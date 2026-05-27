using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;

namespace IdleGame.UI.Support
{
    public static class SupportPanelStateBuilder
    {
        public static string BuildSummary(
            double partyAttackPower,
            double petGoldBonusPercent,
            Func<double, string> formatShort)
        {
            return "Party ATK " + FormatNumber(partyAttackPower, formatShort)
                + "    Pet Gold +" + petGoldBonusPercent.ToString("0.#") + "%";
        }

        public static string BuildSkillStatus(
            CombatSkillState skill,
            double partyAttackPower,
            Func<double, string> formatShort)
        {
            if (skill == null || skill.Definition == null)
            {
                return string.Empty;
            }

            double projectedDamage = partyAttackPower * skill.Definition.PartyAttackMultiplier;
            return skill.Definition.DisplayName
                + "    Cooldown " + Mathf.CeilToInt(skill.CooldownRemaining) + "s"
                + "\nDamage " + FormatNumber(projectedDamage, formatShort)
                + "    Party ATK x" + skill.Definition.PartyAttackMultiplier.ToString("0.0");
        }

        public static string BuildPetStatus(
            PetState pet,
            double petGoldBonusPercent,
            Func<double, string> formatShort)
        {
            if (pet == null || pet.Definition == null)
            {
                return string.Empty;
            }

            return pet.Definition.DisplayName
                + "    Next " + Mathf.CeilToInt(pet.AttackCooldown) + "s"
                + "\nATK " + FormatNumber(pet.Definition.AttackPower, formatShort)
                + "    Interval " + pet.Definition.AttackInterval.ToString("0.0") + "s"
                + "    Gold +" + petGoldBonusPercent.ToString("0.#") + "%";
        }

        public static bool HasReadySkill(IReadOnlyList<CombatSkillState> skills, float readyThresholdSeconds = 0.5f)
        {
            if (skills == null)
            {
                return false;
            }

            for (int i = 0; i < skills.Count; i++)
            {
                CombatSkillState skill = skills[i];
                if (skill != null && skill.CooldownRemaining <= readyThresholdSeconds)
                {
                    return true;
                }
            }

            return false;
        }

        private static string FormatNumber(double value, Func<double, string> formatShort)
        {
            return formatShort != null ? formatShort(value) : value.ToString("0");
        }
    }
}
