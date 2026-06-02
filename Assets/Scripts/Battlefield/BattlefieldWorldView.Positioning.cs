using System.Collections.Generic;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Speed;

namespace IdleGame.Battlefield
{
    public sealed partial class BattlefieldWorldView
    {
        private Vector2 GetHeroFormationPosition(int heroIndex)
        {
            switch (heroIndex % GameData.MaxPartyHeroes)
            {
                case 0:
                    return new Vector2(-0.60f, -0.75f);
                case 1:
                    return new Vector2(0.58f, -0.78f);
                case 2:
                    return new Vector2(-1.30f, -1.34f);
                case 3:
                    return new Vector2(1.28f, -1.36f);
                case 4:
                    return new Vector2(-1.76f, -0.28f);
                case 5:
                    return new Vector2(1.76f, -0.30f);
                case 6:
                    return new Vector2(-0.22f, -1.72f);
                default:
                    return new Vector2(0.86f, -1.70f);
            }
        }

        private Vector2 GetHeroTargetPosition(string heroId, Vector2 fallback)
        {
            int targetIndex = battleManager.GetHeroTargetVisualIndex(heroId);
            if (targetIndex >= 0 && targetIndex < enemyLocalPositions.Count)
            {
                return enemyLocalPositions[targetIndex];
            }

            return FindNearestEnemy(fallback);
        }

        private Vector2 GetHeroDesiredPosition(HeroState hero, int index, Vector2 basePosition, Vector2 targetPosition, bool isAttacking)
        {
            Vector2 toTarget = targetPosition - basePosition;
            Vector2 direction = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : Vector2.up;
            Vector2 tangent = new Vector2(-direction.y, direction.x);
            float time = Time.time;
            float phase = index * 1.17f;
            float attackPush = isAttacking ? 0.42f : 0f;
            float move = Mathf.Max(0.1f, hero.MoveSpeed);

            switch (hero.Definition.Trait)
            {
                case HeroTrait.Melee:
                    return ClampField(basePosition + direction * (0.76f + attackPush) + tangent * Mathf.Sin(time * (1.8f + move * 0.08f) + phase) * 0.18f);
                case HeroTrait.Ranged:
                    return ClampField(basePosition + direction * 0.18f + tangent * Mathf.Sin(time * (1.2f + move * 0.05f) + phase) * 0.48f);
                case HeroTrait.Support:
                    return ClampField(basePosition + new Vector2(Mathf.Cos(time * 0.9f + phase) * 0.36f, Mathf.Sin(time * 1.1f + phase) * 0.24f) + direction * attackPush * 0.45f);
                case HeroTrait.Defense:
                    return ClampField(basePosition + direction * (0.48f + attackPush * 0.55f) + tangent * Mathf.Sin(time * 0.9f + phase) * 0.14f);
                default:
                    return ClampField(basePosition + direction * (0.42f + attackPush));
            }
        }

        private Vector2 GetEnemySpawnPosition(int spawnSequence)
        {
            int side = Mathf.Abs(spawnSequence) % 4;
            float offset = Mathf.Lerp(-2.6f, 2.6f, PseudoRandom01(spawnSequence * 19 + 5));
            switch (side)
            {
                case 0:
                    return new Vector2(-FieldHalfWidth - 0.55f, offset);
                case 1:
                    return new Vector2(FieldHalfWidth + 0.55f, offset);
                case 2:
                    return new Vector2(offset, FieldHalfHeight + 0.55f);
                default:
                    return new Vector2(offset, -FieldHalfHeight - 0.55f);
            }
        }

        private Vector2 GetEnemyDesiredPosition(int index, int spawnSequence, Vector2 currentPosition)
        {
            Vector2 nearestHero = FindNearestHero(currentPosition);
            Vector2 fromCenter = currentPosition.sqrMagnitude > 0.001f ? currentPosition.normalized : GetEnemySpawnPosition(spawnSequence).normalized;
            Vector2 tangent = new Vector2(-fromCenter.y, fromCenter.x);
            float ring = 0.48f + (index % 3) * 0.16f;
            float sideOffset = ((index % 5) - 2) * 0.18f;
            Vector2 swarmOffset = fromCenter * ring + tangent * sideOffset;
            float phase = spawnSequence * 0.73f + index * 0.41f;
            Vector2 idleMotion = fromCenter * Mathf.Sin(Time.time * 1.8f + phase) * 0.12f
                + tangent * Mathf.Cos(Time.time * 2.1f + phase) * 0.16f;
            return ClampField(nearestHero + swarmOffset + idleMotion, 0.35f);
        }

        private Vector2 FindNearestHero(Vector2 fromPosition)
        {
            Vector2 nearest = Vector2.zero;
            float bestDistance = float.MaxValue;
            foreach (Vector2 heroPosition in heroLocalPositions.Values)
            {
                float distance = (fromPosition - heroPosition).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = heroPosition;
                }
            }

            return bestDistance < float.MaxValue ? nearest : Vector2.zero;
        }

        private Vector2 FindNearestEnemy(Vector2 fromPosition)
        {
            Vector2 nearest = Vector2.zero;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < enemyActors.Count; i++)
            {
                if (!enemyActors[i].Root.activeSelf)
                {
                    continue;
                }

                float distance = (fromPosition - enemyActors[i].LocalPosition).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = enemyActors[i].LocalPosition;
                }
            }

            return bestDistance < float.MaxValue ? nearest : Vector2.zero;
        }

        private bool IsHeroInAttackBatch(string heroId)
        {
            IReadOnlyList<string> attackIds = battleManager.RecentHeroAttackIds;
            for (int i = 0; i < attackIds.Count; i++)
            {
                if (attackIds[i] == heroId)
                {
                    return true;
                }
            }

            return false;
        }

        private Vector2 GetHeroAnimationOffset(WorldActor actor, Vector2 currentPosition)
        {
            Vector2 offset = Vector2.zero;
            if (actor.AttackPulse > 0f)
            {
                Vector2 toHit = battleManager.LastHitPosition - currentPosition;
                if (toHit.sqrMagnitude > 0.001f)
                {
                    float pulse = Mathf.Sin(GetPulseRatio(actor.AttackPulse, 0.22f) * Mathf.PI);
                    offset += toHit.normalized * (0.22f * pulse);
                }
            }

            if (actor.HitPulse > 0f)
            {
                float pulse = GetPulseRatio(actor.HitPulse, 0.20f);
                offset += new Vector2(Mathf.Sin(Time.time * 80f), Mathf.Cos(Time.time * 65f)) * (0.05f * pulse);
            }

            return offset;
        }

        private Vector2 GetEnemyAnimationOffset(WorldActor actor, Vector2 currentPosition)
        {
            Vector2 offset = Vector2.zero;
            if (actor.AttackPulse > 0f)
            {
                Vector2 toHero = battleManager.LastMonsterHitPosition - currentPosition;
                if (toHero.sqrMagnitude > 0.001f)
                {
                    float pulse = Mathf.Sin(GetPulseRatio(actor.AttackPulse, 0.18f) * Mathf.PI);
                    offset += toHero.normalized * (0.18f * pulse);
                }
            }

            if (actor.HitPulse > 0f)
            {
                float pulse = GetPulseRatio(actor.HitPulse, 0.20f);
                offset += new Vector2(Mathf.Sin(Time.time * 90f), Mathf.Cos(Time.time * 74f)) * (0.06f * pulse);
            }

            return offset;
        }
    }
}
