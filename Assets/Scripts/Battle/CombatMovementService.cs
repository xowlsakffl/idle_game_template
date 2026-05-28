using System.Collections.Generic;
using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static class CombatMovementService
    {
        public static Vector2 MoveTowardCombatRange(
            Vector2 current,
            Vector2 target,
            float preferredDistance,
            float speed,
            float deltaTime,
            float fieldHalfWidth,
            float fieldHalfHeight)
        {
            Vector2 toTarget = target - current;
            float distance = toTarget.magnitude;
            if (distance <= 0.001f)
            {
                return current;
            }

            Vector2 desired = distance > preferredDistance
                ? target - toTarget.normalized * preferredDistance
                : current;
            return ClampBattlePosition(Vector2.MoveTowards(current, desired, speed * deltaTime), fieldHalfWidth, fieldHalfHeight);
        }

        public static void ApplyHeroSeparation(
            IReadOnlyList<HeroState> deployedHeroes,
            IDictionary<string, BattleHeroRuntimeState> heroRuntimeStates,
            float minDistance,
            float fieldHalfWidth,
            float fieldHalfHeight)
        {
            var states = new List<BattleHeroRuntimeState>();
            if (deployedHeroes == null || heroRuntimeStates == null)
            {
                return;
            }

            for (int i = 0; i < deployedHeroes.Count; i++)
            {
                HeroState hero = deployedHeroes[i];
                if (heroRuntimeStates.TryGetValue(hero.Definition.Id, out BattleHeroRuntimeState state) && state.IsAlive)
                {
                    states.Add(state);
                }
            }

            for (int i = 0; i < states.Count; i++)
            {
                for (int j = i + 1; j < states.Count; j++)
                {
                    PushActorsApart(states[i], states[j], minDistance, i, j, fieldHalfWidth, fieldHalfHeight);
                }
            }
        }

        public static void ApplyEnemySeparation(
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            float minDistance,
            float fieldHalfWidth,
            float fieldHalfHeight)
        {
            if (visibleEnemies == null)
            {
                return;
            }

            for (int i = 0; i < visibleEnemies.Count; i++)
            {
                VisibleEnemyState left = visibleEnemies[i];
                if (left.Hp <= GameNumber.Zero)
                {
                    continue;
                }

                for (int j = i + 1; j < visibleEnemies.Count; j++)
                {
                    VisibleEnemyState right = visibleEnemies[j];
                    if (right.Hp <= GameNumber.Zero)
                    {
                        continue;
                    }

                    PushEnemiesApart(left, right, minDistance, i, j, fieldHalfWidth, fieldHalfHeight);
                }
            }
        }

        public static Vector2 GetHeroBattleSlotPosition(HeroState hero, int heroIndex)
        {
            if (IsFortressProtectedHero(hero))
            {
                switch (heroIndex % GameData.MaxPartyHeroes)
                {
                    case 0:
                        return new Vector2(-0.36f, -0.08f);
                    case 1:
                        return new Vector2(0.36f, -0.08f);
                    case 2:
                        return new Vector2(-0.34f, 0.42f);
                    case 3:
                        return new Vector2(0.34f, 0.42f);
                    case 4:
                        return new Vector2(0f, -0.52f);
                    case 5:
                        return new Vector2(-0.58f, 0.18f);
                    case 6:
                        return new Vector2(0.58f, 0.18f);
                    default:
                        return new Vector2(0f, 0.70f);
                }
            }

            switch (heroIndex % GameData.MaxPartyHeroes)
            {
                case 0:
                    return new Vector2(-0.92f, -0.58f);
                case 1:
                    return new Vector2(0.92f, -0.58f);
                case 2:
                    return new Vector2(-1.42f, 0.12f);
                case 3:
                    return new Vector2(1.42f, 0.12f);
                case 4:
                    return new Vector2(0f, -1.18f);
                case 5:
                    return new Vector2(-1.82f, -0.44f);
                case 6:
                    return new Vector2(1.82f, -0.44f);
                default:
                    return new Vector2(0f, 1.10f);
            }
        }

        public static bool IsFortressProtectedHero(HeroState hero)
        {
            return hero != null
                && (hero.Definition.Trait == HeroTrait.Ranged || hero.Definition.Trait == HeroTrait.Support);
        }

        public static float GetHeroAttackRange(HeroState hero)
        {
            switch (hero.Definition.Trait)
            {
                case HeroTrait.Melee:
                    return 0.82f;
                case HeroTrait.Ranged:
                    return 4.35f;
                case HeroTrait.Support:
                    return 3.65f;
                case HeroTrait.Defense:
                    return 0.92f;
                default:
                    return 1.25f;
            }
        }

        public static float GetEnemyMoveSpeed(VisibleEnemyState enemy)
        {
            return 1.15f + (Mathf.Abs(enemy.SpawnSequence) % 4) * 0.09f;
        }

        private static void PushActorsApart(
            BattleHeroRuntimeState left,
            BattleHeroRuntimeState right,
            float minDistance,
            int leftIndex,
            int rightIndex,
            float fieldHalfWidth,
            float fieldHalfHeight)
        {
            Vector2 delta = right.Position - left.Position;
            float distance = delta.magnitude;
            if (distance >= minDistance)
            {
                return;
            }

            Vector2 direction = distance > 0.001f ? delta / distance : GetFallbackSeparationDirection(leftIndex, rightIndex);
            float push = (minDistance - distance) * 0.5f;
            left.Position = ClampBattlePosition(left.Position - direction * push, fieldHalfWidth, fieldHalfHeight);
            right.Position = ClampBattlePosition(right.Position + direction * push, fieldHalfWidth, fieldHalfHeight);
        }

        private static void PushEnemiesApart(
            VisibleEnemyState left,
            VisibleEnemyState right,
            float minDistance,
            int leftIndex,
            int rightIndex,
            float fieldHalfWidth,
            float fieldHalfHeight)
        {
            Vector2 delta = right.Position - left.Position;
            float distance = delta.magnitude;
            if (distance >= minDistance)
            {
                return;
            }

            Vector2 direction = distance > 0.001f ? delta / distance : GetFallbackSeparationDirection(leftIndex, rightIndex);
            float push = (minDistance - distance) * 0.5f;
            left.Position = ClampBattlePosition(left.Position - direction * push, fieldHalfWidth, fieldHalfHeight);
            right.Position = ClampBattlePosition(right.Position + direction * push, fieldHalfWidth, fieldHalfHeight);
        }

        private static Vector2 GetFallbackSeparationDirection(int leftIndex, int rightIndex)
        {
            float angle = (leftIndex * 37f + rightIndex * 53f) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
        }

        private static Vector2 ClampBattlePosition(Vector2 position, float fieldHalfWidth, float fieldHalfHeight)
        {
            return new Vector2(
                Mathf.Clamp(position.x, -fieldHalfWidth + 0.12f, fieldHalfWidth - 0.12f),
                Mathf.Clamp(position.y, -fieldHalfHeight + 0.12f, fieldHalfHeight - 0.12f));
        }
    }
}
