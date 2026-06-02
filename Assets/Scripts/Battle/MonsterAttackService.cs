using System;
using System.Collections.Generic;
using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static class MonsterAttackService
    {
        public static void TickEnemyAttacks(
            bool isBossFight,
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            IEnumerable<BattleHeroRuntimeState> heroStates,
            Dictionary<string, int> heroTargetLocks,
            ref GameNumber fortressHp,
            GameNumber fortressMaxHp,
            double damageTakenMultiplier,
            float deltaTime,
            float enemyAttackRange,
            float fortressEnemyAttackRange,
            float attackInterval,
            float retryCooldown,
            float reviveSeconds,
            Action<CombatHitService.MonsterHitResult> applyHitResult)
        {
            if (isBossFight || visibleEnemies == null || visibleEnemies.Count <= 0)
            {
                return;
            }

            bool fortressAlive = fortressHp > GameNumber.Zero;
            for (int i = 0; i < visibleEnemies.Count; i++)
            {
                VisibleEnemyState enemy = visibleEnemies[i];
                if (enemy == null || !enemy.IsAttackable)
                {
                    continue;
                }

                BattleHeroRuntimeState targetHero = CombatTargetingService.FindNearestMonsterTargetHero(
                    heroStates,
                    enemy.Position,
                    fortressAlive);
                if (targetHero == null)
                {
                    TickFortressTarget(
                        i,
                        enemy,
                        ref fortressHp,
                        fortressMaxHp,
                        deltaTime,
                        fortressEnemyAttackRange,
                        attackInterval,
                        retryCooldown,
                        applyHitResult);
                    fortressAlive = fortressHp > GameNumber.Zero;
                    continue;
                }

                TickHeroTarget(
                    i,
                    enemy,
                    targetHero,
                    heroTargetLocks,
                    damageTakenMultiplier,
                    deltaTime,
                    enemyAttackRange,
                    attackInterval,
                    retryCooldown,
                    reviveSeconds,
                    applyHitResult);
            }
        }

        private static void TickFortressTarget(
            int enemyIndex,
            VisibleEnemyState enemy,
            ref GameNumber fortressHp,
            GameNumber fortressMaxHp,
            float deltaTime,
            float attackRange,
            float attackInterval,
            float retryCooldown,
            Action<CombatHitService.MonsterHitResult> applyHitResult)
        {
            if (fortressHp <= GameNumber.Zero
                || !CombatTickService.TryTickEnemyAttack(
                    enemy,
                    Vector2.zero,
                    attackRange,
                    deltaTime,
                    attackInterval,
                    retryCooldown))
            {
                return;
            }

            CombatHitService.MonsterHitResult hitResult = CombatHitService.ApplyMonsterDamageToFortress(
                enemyIndex,
                fortressHp,
                fortressMaxHp);
            if (!hitResult.Applied)
            {
                return;
            }

            fortressHp = hitResult.FortressHp;
            applyHitResult?.Invoke(hitResult);
        }

        private static void TickHeroTarget(
            int enemyIndex,
            VisibleEnemyState enemy,
            BattleHeroRuntimeState targetHero,
            Dictionary<string, int> heroTargetLocks,
            double damageTakenMultiplier,
            float deltaTime,
            float attackRange,
            float attackInterval,
            float retryCooldown,
            float reviveSeconds,
            Action<CombatHitService.MonsterHitResult> applyHitResult)
        {
            if (!CombatTickService.TryTickEnemyAttack(
                    enemy,
                    targetHero.Position,
                    attackRange,
                    deltaTime,
                    attackInterval,
                    retryCooldown))
            {
                return;
            }

            CombatHitService.MonsterHitResult hitResult = CombatHitService.ApplyMonsterDamageToHero(
                enemyIndex,
                targetHero,
                damageTakenMultiplier,
                reviveSeconds);
            if (!hitResult.Applied)
            {
                return;
            }

            if (hitResult.HeroDefeated && !string.IsNullOrEmpty(hitResult.HeroId))
            {
                heroTargetLocks?.Remove(hitResult.HeroId);
            }

            applyHitResult?.Invoke(hitResult);
        }
    }
}
