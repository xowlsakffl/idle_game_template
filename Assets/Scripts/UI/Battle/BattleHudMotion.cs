using UnityEngine;
using IdleGame.Data;

namespace IdleGame.UI.Battle
{
    public static class BattleHudMotion
    {
        public static float GetClampedDeltaTime(float deltaTime)
        {
            if (deltaTime <= 0f || float.IsNaN(deltaTime))
            {
                deltaTime = 1f / 60f;
            }

            return Mathf.Min(deltaTime, 0.033f);
        }

        public static Vector2 ClampBattlefieldPosition(Vector2 position, float fieldWidth, float fieldHeight, float margin)
        {
            float halfWidth = Mathf.Max(0f, fieldWidth * 0.5f - margin);
            float halfHeight = Mathf.Max(0f, fieldHeight * 0.5f - margin);
            return new Vector2(
                Mathf.Clamp(position.x, -halfWidth, halfWidth),
                Mathf.Clamp(position.y, -halfHeight, halfHeight));
        }

        public static float GetEnemyApproachRatio(Vector2 spawnPosition, Vector2 targetPosition, Vector2 currentPosition)
        {
            float totalDistance = Vector2.Distance(spawnPosition, targetPosition);
            if (totalDistance <= 0.001f)
            {
                return 1f;
            }

            float remainingDistance = Vector2.Distance(currentPosition, targetPosition);
            return Mathf.Clamp01(1f - remainingDistance / totalDistance);
        }

        public static Vector2 GetEnemySpreadDirection(int index)
        {
            float angle = index * 137.5f * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            return direction.sqrMagnitude <= 0.001f ? Vector2.right : direction.normalized;
        }

        public static Vector2 GetEnemyAggroOffset(int index, Vector2 spawnDirection, float time)
        {
            Vector2 tangent = new Vector2(-spawnDirection.y, spawnDirection.x);
            float side = ((index % 5) - 2) * 18f;
            float ring = 54f + (index % 3) * 12f + Mathf.Sin(time * 2.1f + index) * 7f;
            return spawnDirection * ring + tangent * side;
        }

        public static Vector2 GetHeroRoamOffset(HeroState hero, int heroIndex, float time, float fieldWidth, float fieldHeight)
        {
            float phase = heroIndex * 1.37f;
            float move = Mathf.Max(0.1f, hero.MoveSpeed);
            float xRadius = Mathf.Min(116f + move * 8f, fieldWidth * 0.20f);
            float yRadius = Mathf.Min(58f + move * 6f, fieldHeight * 0.18f);

            switch (hero.Definition.Trait)
            {
                case HeroTrait.Melee:
                {
                    float patrol = Mathf.Sin(time * (0.64f + move * 0.045f) + phase);
                    float weave = Mathf.Sin(time * (0.92f + move * 0.05f) + phase * 0.7f);
                    return new Vector2(weave * xRadius * 0.48f, 18f + patrol * yRadius * 0.78f);
                }
                case HeroTrait.Ranged:
                {
                    float strafe = Mathf.Sin(time * (0.52f + move * 0.04f) + phase);
                    float backStep = Mathf.Cos(time * (0.36f + move * 0.03f) + phase);
                    return new Vector2(strafe * xRadius * 0.88f, -32f + backStep * yRadius * 0.42f);
                }
                case HeroTrait.Support:
                {
                    float orbitSpeed = 0.42f + move * 0.035f;
                    return new Vector2(
                        Mathf.Cos(time * orbitSpeed + phase) * xRadius * 0.52f,
                        Mathf.Sin(time * (orbitSpeed + 0.18f) + phase) * yRadius * 0.64f);
                }
                case HeroTrait.Defense:
                {
                    float guardPatrol = Mathf.Sin(time * (0.38f + move * 0.025f) + phase);
                    float braceShift = Mathf.Sin(time * (0.78f + move * 0.04f) + phase * 0.5f);
                    return new Vector2(guardPatrol * xRadius * 0.35f, -2f + braceShift * yRadius * 0.36f);
                }
                default:
                {
                    return new Vector2(
                        Mathf.Sin(time * 0.6f + phase) * xRadius * 0.45f,
                        Mathf.Cos(time * 0.5f + phase) * yRadius * 0.45f);
                }
            }
        }

        public static Vector2 GetHeroTraitMotionOffset(HeroState hero, int heroIndex, float time, bool isLastSource, float flashRatio)
        {
            return Vector2.zero;
        }

        public static float GetHeroTraitScale(HeroState hero, bool isLastSource, float flashRatio, float time, int heroIndex)
        {
            float phase = heroIndex * 0.73f;
            float hit = isLastSource ? Mathf.Clamp01(flashRatio) : 0f;

            switch (hero.Definition.Trait)
            {
                case HeroTrait.Melee:
                    return 1f + Mathf.Max(0f, Mathf.Sin(time * (4f + hero.AttackSpeed) + phase)) * 0.035f + 0.2f * hit;
                case HeroTrait.Ranged:
                    return 0.96f + Mathf.Sin(time * 1.7f + phase) * 0.015f + 0.12f * hit;
                case HeroTrait.Support:
                    return 0.98f + Mathf.Sin(time * 2.2f + phase) * 0.035f + 0.14f * hit;
                case HeroTrait.Defense:
                    return 1.07f + Mathf.Sin(time * 1.1f + phase) * 0.012f + 0.1f * hit;
                default:
                    return 1f + 0.18f * hit;
            }
        }

        public static Vector2 GetHeroPursuitOffset(
            HeroState hero,
            int heroIndex,
            Vector2 fromPosition,
            Vector2 enemyPosition,
            float time,
            bool isAttackSource,
            float flashRatio)
        {
            Vector2 toEnemy = enemyPosition - fromPosition;
            if (toEnemy.sqrMagnitude <= 0.001f)
            {
                return Vector2.zero;
            }

            Vector2 direction = toEnemy.normalized;
            Vector2 tangent = new Vector2(-direction.y, direction.x);
            float phase = heroIndex * 0.91f;
            float move = Mathf.Max(0.1f, hero.MoveSpeed);
            float distance = toEnemy.magnitude;

            switch (hero.Definition.Trait)
            {
                case HeroTrait.Melee:
                {
                    float chase = Mathf.Min(distance - 34f, 74f + move * 7f);
                    float weave = Mathf.Sin(time * (2.4f + move * 0.18f) + phase) * 10f;
                    return direction * Mathf.Max(0f, chase) + tangent * weave;
                }
                case HeroTrait.Ranged:
                {
                    float preferredDistance = 158f;
                    float adjust = Mathf.Clamp(distance - preferredDistance, -36f, 46f);
                    float strafe = Mathf.Sin(time * (1.5f + move * 0.11f) + phase) * (24f + move * 2f);
                    return direction * adjust + tangent * strafe;
                }
                case HeroTrait.Support:
                {
                    float preferredDistance = 118f;
                    float adjust = Mathf.Clamp(distance - preferredDistance, -28f, 36f);
                    float orbit = Mathf.Sin(time * (1.15f + move * 0.08f) + phase) * 22f;
                    return direction * adjust + tangent * orbit;
                }
                case HeroTrait.Defense:
                {
                    float chase = Mathf.Min(distance - 54f, 42f + move * 4f);
                    float guard = Mathf.Sin(time * (0.95f + move * 0.06f) + phase) * 8f;
                    return direction * Mathf.Max(0f, chase) + tangent * guard;
                }
                default:
                {
                    float chase = Mathf.Clamp(distance - 92f, 0f, 52f + move * 4f);
                    return direction * chase;
                }
            }
        }
    }
}
