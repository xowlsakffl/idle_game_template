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

        public static void TickReadySkills(
            IReadOnlyList<CombatSkillState> skills,
            float deltaTime,
            Func<bool> hasAttackableTarget,
            float cooldownMultiplier,
            Action<CombatSkillState> castSkill)
        {
            if (skills == null || hasAttackableTarget == null || castSkill == null || !hasAttackableTarget())
            {
                return;
            }

            foreach (CombatSkillState skill in skills)
            {
                if (skill == null)
                {
                    continue;
                }

                skill.CooldownRemaining -= deltaTime;
                if (skill.CooldownRemaining > 0f)
                {
                    continue;
                }

                skill.CooldownRemaining += skill.Definition.CooldownSeconds * cooldownMultiplier;
                castSkill(skill);
                if (!hasAttackableTarget())
                {
                    return;
                }
            }
        }

        public static void TickReadyPets(
            IReadOnlyList<PetState> pets,
            float deltaTime,
            Func<bool> hasAttackableTarget,
            Action<PetState> attackWithPet)
        {
            if (pets == null || hasAttackableTarget == null || attackWithPet == null || !hasAttackableTarget())
            {
                return;
            }

            foreach (PetState pet in pets)
            {
                if (pet == null)
                {
                    continue;
                }

                pet.AttackCooldown -= deltaTime;
                if (pet.AttackCooldown > 0f)
                {
                    continue;
                }

                pet.AttackCooldown += pet.Definition.AttackInterval;
                attackWithPet(pet);
                if (!hasAttackableTarget())
                {
                    return;
                }
            }
        }

        public static bool TryTickFortressAttack(
            GameNumber fortressHp,
            bool hasAttackableTarget,
            bool isBossFight,
            ref float attackCooldown,
            float deltaTime,
            float attackInterval,
            Func<int> findTargetIndex,
            float retryCooldown,
            out int targetIndex)
        {
            targetIndex = -1;
            if (fortressHp <= GameNumber.Zero || !hasAttackableTarget)
            {
                return false;
            }

            attackCooldown -= deltaTime;
            if (attackCooldown > 0f)
            {
                return false;
            }

            if (isBossFight)
            {
                attackCooldown += attackInterval;
                return true;
            }

            targetIndex = findTargetIndex?.Invoke() ?? -1;
            if (targetIndex < 0)
            {
                attackCooldown = Mathf.Min(attackCooldown, retryCooldown);
                return false;
            }

            attackCooldown += attackInterval;
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
