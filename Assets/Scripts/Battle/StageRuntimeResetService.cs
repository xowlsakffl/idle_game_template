using System.Collections.Generic;
using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static class StageRuntimeResetService
    {
        public static void ResetCombatantCooldowns(
            IReadOnlyList<HeroState> heroes,
            IReadOnlyList<CombatSkillState> skills,
            IReadOnlyList<PetState> pets,
            float initialSpawnGraceSeconds,
            double skillCooldownMultiplier,
            float fortressAttackInterval,
            out float fortressAttackCooldown)
        {
            ResetHeroCooldowns(heroes, initialSpawnGraceSeconds);
            ResetSkillCooldowns(skills, skillCooldownMultiplier);
            ResetPetCooldowns(pets, initialSpawnGraceSeconds);
            fortressAttackCooldown = Mathf.Min(fortressAttackInterval, initialSpawnGraceSeconds + 0.15f);
        }

        private static void ResetHeroCooldowns(IReadOnlyList<HeroState> heroes, float initialSpawnGraceSeconds)
        {
            if (heroes == null)
            {
                return;
            }

            foreach (HeroState hero in heroes)
            {
                if (hero != null)
                {
                    hero.AttackCooldown = Mathf.Min(hero.AttackInterval, initialSpawnGraceSeconds + 0.1f);
                }
            }
        }

        private static void ResetSkillCooldowns(IReadOnlyList<CombatSkillState> skills, double skillCooldownMultiplier)
        {
            if (skills == null)
            {
                return;
            }

            foreach (CombatSkillState skill in skills)
            {
                if (skill != null)
                {
                    skill.CooldownRemaining = skill.Definition.CooldownSeconds * (float)skillCooldownMultiplier;
                }
            }
        }

        private static void ResetPetCooldowns(IReadOnlyList<PetState> pets, float initialSpawnGraceSeconds)
        {
            if (pets == null)
            {
                return;
            }

            foreach (PetState pet in pets)
            {
                if (pet != null)
                {
                    pet.AttackCooldown = Mathf.Min(pet.Definition.AttackInterval, initialSpawnGraceSeconds + 0.2f);
                }
            }
        }
    }
}
