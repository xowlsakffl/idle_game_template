using System;
using System.Collections.Generic;
using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static partial class CombatMovementService
    {
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
                if (IsFortressProtectedHero(hero))
                {
                    continue;
                }

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
