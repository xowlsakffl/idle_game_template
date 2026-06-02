using System;
using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.Battle
{
    internal static class CombatAttackService
    {
        public static CombatAttackAction BuildHeroAttack(
            HeroState hero,
            bool isBossFight,
            int visibleEnemyIndex,
            AbilityManager abilityManager,
            double heroOwnedAttackMultiplier,
            double attackTalentMultiplier,
            double criticalDamageTalentMultiplier,
            double finalDamageTalentMultiplier,
            double totemAttackMultiplier,
            double runeAttackMultiplier,
            double totemCriticalChanceBonusPercent,
            double runeCriticalChanceBonusPercent,
            double runeFinalDamageMultiplier,
            Func<double> random01)
        {
            if (hero == null)
            {
                return CombatAttackAction.Invalid;
            }

            GameNumber damage = CombatDamageService.CalculateHeroDamage(
                hero,
                abilityManager,
                heroOwnedAttackMultiplier,
                attackTalentMultiplier,
                criticalDamageTalentMultiplier,
                finalDamageTalentMultiplier,
                totemAttackMultiplier,
                runeAttackMultiplier,
                totemCriticalChanceBonusPercent,
                runeCriticalChanceBonusPercent,
                runeFinalDamageMultiplier,
                random01,
                out bool isCritical);
            return new CombatAttackAction(
                true,
                isBossFight,
                visibleEnemyIndex,
                damage,
                hero.Definition.DisplayName,
                isCritical,
                hero.Definition.Id);
        }

        public static CombatAttackAction BuildSkillAttack(
            CombatSkillState skill,
            bool isBossFight,
            int visibleEnemyIndex,
            double partyAttackPower,
            AbilityManager abilityManager,
            double finalDamageTalentMultiplier,
            double skillDamageTalentMultiplier,
            double totemSkillDamageMultiplier,
            double runeSkillDamageMultiplier)
        {
            if (skill == null)
            {
                return CombatAttackAction.Invalid;
            }

            GameNumber damage = CombatDamageService.CalculateSkillDamage(
                partyAttackPower,
                skill,
                abilityManager,
                finalDamageTalentMultiplier,
                skillDamageTalentMultiplier,
                totemSkillDamageMultiplier,
                runeSkillDamageMultiplier);
            return new CombatAttackAction(
                true,
                isBossFight,
                visibleEnemyIndex,
                damage,
                skill.Definition.DisplayName,
                false,
                null);
        }

        public static CombatAttackAction BuildPetAttack(
            PetState pet,
            bool isBossFight,
            int visibleEnemyIndex,
            AbilityManager abilityManager,
            double finalDamageTalentMultiplier)
        {
            if (pet == null)
            {
                return CombatAttackAction.Invalid;
            }

            GameNumber damage = CombatDamageService.CalculatePetDamage(
                pet,
                abilityManager,
                finalDamageTalentMultiplier);
            return new CombatAttackAction(
                true,
                isBossFight,
                visibleEnemyIndex,
                damage,
                pet.Definition.DisplayName,
                false,
                null);
        }
    }
}
