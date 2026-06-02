using System;
using System.Collections.Generic;
using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static partial class CombatMovementService
    {
        public static void TickHeroMovement(
            IReadOnlyList<HeroState> deployedHeroes,
            IDictionary<string, BattleHeroRuntimeState> heroRuntimeStates,
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            IDictionary<string, int> heroTargetSpawnSequences,
            float deltaTime,
            Func<HeroState, int, Vector2> getSlotPosition,
            Func<HeroState, float> getMoveSpeed,
            float heroSeparationRadius,
            float fieldHalfWidth,
            float fieldHalfHeight,
            float reviveAttackCooldown)
        {
            if (deployedHeroes == null || heroRuntimeStates == null)
            {
                return;
            }

            for (int i = 0; i < deployedHeroes.Count; i++)
            {
                HeroState hero = deployedHeroes[i];
                if (hero == null || !heroRuntimeStates.TryGetValue(hero.Definition.Id, out BattleHeroRuntimeState heroState))
                {
                    continue;
                }

                TickHeroMovement(
                    hero,
                    heroState,
                    i,
                    visibleEnemies,
                    heroTargetSpawnSequences,
                    deltaTime,
                    getSlotPosition,
                    getMoveSpeed,
                    fieldHalfWidth,
                    fieldHalfHeight,
                    reviveAttackCooldown);
            }

            ApplyHeroSeparation(deployedHeroes, heroRuntimeStates, heroSeparationRadius, fieldHalfWidth, fieldHalfHeight);
        }

        public static void TickEnemyMovement(
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            IEnumerable<BattleHeroRuntimeState> heroRuntimeStates,
            bool fortressAlive,
            bool isBossFight,
            float deltaTime,
            Func<VisibleEnemyState, float> getEnemyMoveSpeed,
            float enemyAttackRange,
            float fortressEnemyAttackRange,
            float enemySeparationRadius,
            float fieldHalfWidth,
            float fieldHalfHeight)
        {
            if (isBossFight || visibleEnemies == null)
            {
                return;
            }

            foreach (VisibleEnemyState enemy in visibleEnemies)
            {
                if (enemy == null || enemy.Hp <= GameNumber.Zero)
                {
                    continue;
                }

                BattleHeroRuntimeState targetHero = CombatTargetingService.FindNearestMonsterTargetHero(
                    heroRuntimeStates,
                    enemy.Position,
                    fortressAlive);
                if (targetHero == null)
                {
                    enemy.TargetHeroId = string.Empty;
                    enemy.Position = MoveTowardCombatRange(
                        enemy.Position,
                        Vector2.zero,
                        fortressEnemyAttackRange * 0.82f,
                        getEnemyMoveSpeed?.Invoke(enemy) ?? GetEnemyMoveSpeed(enemy),
                        deltaTime,
                        fieldHalfWidth,
                        fieldHalfHeight);
                    continue;
                }

                enemy.TargetHeroId = targetHero.Hero.Definition.Id;
                enemy.Position = MoveTowardCombatRange(
                    enemy.Position,
                    targetHero.Position,
                    enemyAttackRange * 0.82f,
                    getEnemyMoveSpeed?.Invoke(enemy) ?? GetEnemyMoveSpeed(enemy),
                    deltaTime,
                    fieldHalfWidth,
                    fieldHalfHeight);
            }

            ApplyEnemySeparation(visibleEnemies, enemySeparationRadius, fieldHalfWidth, fieldHalfHeight);
        }

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
    }
}
