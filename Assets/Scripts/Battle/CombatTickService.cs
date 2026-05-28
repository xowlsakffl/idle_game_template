using System;
using System.Collections.Generic;
using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static class CombatTickService
    {
        public static void TickVisibleEnemySpawnGrace(
            bool isBossFight,
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            float deltaTime)
        {
            if (isBossFight || visibleEnemies == null || visibleEnemies.Count <= 0)
            {
                return;
            }

            foreach (VisibleEnemyState enemy in visibleEnemies)
            {
                if (enemy.SpawnGraceRemaining > 0f)
                {
                    enemy.SpawnGraceRemaining = Mathf.Max(0f, enemy.SpawnGraceRemaining - deltaTime);
                }
            }
        }

        public static bool CollectReadyHeroAttacks(
            IReadOnlyList<HeroState> deployedHeroes,
            IList<HeroState> readyHeroAttacks,
            IList<string> recentHeroAttackIds,
            float deltaTime,
            Func<string, bool> isHeroAlive,
            Func<HeroState, bool> hasAttackTarget,
            Func<HeroState, float> attackInterval,
            float retryCooldown)
        {
            readyHeroAttacks.Clear();
            recentHeroAttackIds.Clear();
            if (deployedHeroes == null)
            {
                return false;
            }

            foreach (HeroState hero in deployedHeroes)
            {
                if (hero == null || isHeroAlive == null || !isHeroAlive(hero.Definition.Id))
                {
                    continue;
                }

                hero.AttackCooldown -= deltaTime;
                if (hero.AttackCooldown > 0f)
                {
                    continue;
                }

                if (hasAttackTarget == null || !hasAttackTarget(hero))
                {
                    hero.AttackCooldown = Mathf.Min(hero.AttackCooldown, retryCooldown);
                    continue;
                }

                hero.AttackCooldown += attackInterval?.Invoke(hero) ?? hero.AttackInterval;
                readyHeroAttacks.Add(hero);
            }

            if (readyHeroAttacks.Count <= 0)
            {
                return false;
            }

            foreach (HeroState hero in readyHeroAttacks)
            {
                recentHeroAttackIds.Add(hero.Definition.Id);
            }

            return true;
        }

        public static bool TryTickEnemyAttack(
            VisibleEnemyState enemy,
            Vector2 targetPosition,
            float attackRange,
            float deltaTime,
            float attackInterval,
            float retryCooldown)
        {
            if (enemy == null)
            {
                return false;
            }

            float attackRangeSqr = attackRange * attackRange;
            float distanceSqr = (enemy.Position - targetPosition).sqrMagnitude;
            if (distanceSqr > attackRangeSqr)
            {
                enemy.AttackCooldown = Mathf.Min(enemy.AttackCooldown, retryCooldown);
                return false;
            }

            enemy.AttackCooldown -= deltaTime;
            if (enemy.AttackCooldown > 0f)
            {
                return false;
            }

            enemy.AttackCooldown += attackInterval;
            return true;
        }
    }
}
