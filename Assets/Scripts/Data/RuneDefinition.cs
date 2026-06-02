using System.Collections.Generic;
using System;
using UnityEngine;

namespace IdleGame.Data
{

    [Serializable]
    public sealed partial class RuneDefinition
    {
        public const int MaxLevel = 50;
        public const RuneGrade MaxGrade = RuneGrade.Mythic;

        public RuneDefinition(
            string id,
            string displayName,
            string icon,
            string role,
            RuneEffectKind effectKind,
            bool startUnlocked = true)
        {
            Id = id;
            DisplayName = displayName;
            Icon = icon;
            Role = role;
            EffectKind = effectKind;
            StartUnlocked = startUnlocked;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Icon { get; }
        public string Role { get; }
        public RuneEffectKind EffectKind { get; }
        public bool StartUnlocked { get; }

        public int GetPromoteRequirement(RuneGrade grade)
        {
            if (grade >= MaxGrade)
            {
                return 0;
            }

            switch (grade)
            {
                case RuneGrade.Common:
                    return 10;
                case RuneGrade.Uncommon:
                    return 30;
                case RuneGrade.Rare:
                    return 80;
                case RuneGrade.Epic:
                    return 200;
                case RuneGrade.Legendary:
                    return 500;
                default:
                    return 0;
            }
        }

        public static string GetGradeLabel(RuneGrade grade)
        {
            switch (grade)
            {
                case RuneGrade.Common:
                    return "커먼";
                case RuneGrade.Uncommon:
                    return "언커먼";
                case RuneGrade.Rare:
                    return "레어";
                case RuneGrade.Epic:
                    return "에픽";
                case RuneGrade.Legendary:
                    return "전설";
                case RuneGrade.Mythic:
                    return "신화";
                default:
                    return grade.ToString();
            }
        }

        public static float GetGradePower(RuneGrade grade)
        {
            switch (grade)
            {
                case RuneGrade.Common:
                    return 1.0f;
                case RuneGrade.Uncommon:
                    return 1.6f;
                case RuneGrade.Rare:
                    return 2.4f;
                case RuneGrade.Epic:
                    return 3.4f;
                case RuneGrade.Legendary:
                    return 4.6f;
                case RuneGrade.Mythic:
                    return 6.0f;
                default:
                    return 1.0f;
            }
        }

        public string GetEffectSummary(RuneGrade grade)
        {
            switch (EffectKind)
            {
                case RuneEffectKind.Strike:
                    return "공격력 +" + GetAttackPercent(grade).ToString("0.##") + "%";
                case RuneEffectKind.Execute:
                    return "최종 피해 +" + GetFinalDamagePercent(grade).ToString("0.##") + "%";
                case RuneEffectKind.Barrier:
                    return "체력 +" + GetHpPercent(grade).ToString("0.##") + "%\n받는 피해 -" + GetDamageReductionPercent(grade).ToString("0.##") + "%";
                case RuneEffectKind.Harvest:
                    return "골드 +" + GetGoldGainPercent(grade).ToString("0.##") + "%\n경험치책 +" + GetHeroExpGainPercent(grade).ToString("0.##") + "%";
                case RuneEffectKind.Arcane:
                    return "스킬 피해 +" + GetSkillDamagePercent(grade).ToString("0.##") + "%\n스킬 쿨타임 -" + GetSkillCooldownReductionPercent(grade).ToString("0.##") + "%";
                case RuneEffectKind.Storm:
                    return "공속 +" + GetAttackSpeedPercent(grade).ToString("0.##") + "%\n이속 +" + GetMoveSpeedPercent(grade).ToString("0.##") + "%";
                case RuneEffectKind.Focus:
                    return "치명타 확률 +" + GetCriticalChancePercent(grade).ToString("0.##") + "%";
                case RuneEffectKind.Vitality:
                    return "체력 +" + GetHpPercent(grade).ToString("0.##") + "%";
                case RuneEffectKind.Command:
                    return "공격력 +" + GetAttackPercent(grade).ToString("0.##") + "%\n계정 경험치 +" + GetAccountExpGainPercent(grade).ToString("0.##") + "%";
                case RuneEffectKind.Regeneration:
                    return "받는 피해 -" + GetDamageReductionPercent(grade).ToString("0.##") + "%";
                default:
                    return Role;
            }
        }

        public double GetAttackPercent(RuneGrade grade)
        {
            double power = GetGradePower(grade);
            switch (EffectKind)
            {
                case RuneEffectKind.Strike:
                    return power * 0.35d;
                case RuneEffectKind.Command:
                    return power * 0.20d;
                default:
                    return 0d;
            }
        }

        public double GetFinalDamagePercent(RuneGrade grade)
        {
            return EffectKind == RuneEffectKind.Execute ? GetGradePower(grade) * 0.22d : 0d;
        }

        public double GetHpPercent(RuneGrade grade)
        {
            double power = GetGradePower(grade);
            switch (EffectKind)
            {
                case RuneEffectKind.Barrier:
                    return power * 0.32d;
                case RuneEffectKind.Vitality:
                    return power * 0.45d;
                default:
                    return 0d;
            }
        }

        public double GetDamageReductionPercent(RuneGrade grade)
        {
            double power = GetGradePower(grade);
            switch (EffectKind)
            {
                case RuneEffectKind.Barrier:
                    return power * 0.08d;
                case RuneEffectKind.Regeneration:
                    return power * 0.07d;
                default:
                    return 0d;
            }
        }

        public double GetGoldGainPercent(RuneGrade grade)
        {
            return EffectKind == RuneEffectKind.Harvest ? GetGradePower(grade) * 0.40d : 0d;
        }

        public double GetHeroExpGainPercent(RuneGrade grade)
        {
            return EffectKind == RuneEffectKind.Harvest ? GetGradePower(grade) * 0.25d : 0d;
        }

        public double GetAccountExpGainPercent(RuneGrade grade)
        {
            return EffectKind == RuneEffectKind.Command ? GetGradePower(grade) * 0.28d : 0d;
        }

        public double GetAttackSpeedPercent(RuneGrade grade)
        {
            return EffectKind == RuneEffectKind.Storm ? GetGradePower(grade) * 0.22d : 0d;
        }

        public double GetMoveSpeedPercent(RuneGrade grade)
        {
            return EffectKind == RuneEffectKind.Storm ? GetGradePower(grade) * 0.18d : 0d;
        }

        public double GetSkillDamagePercent(RuneGrade grade)
        {
            return EffectKind == RuneEffectKind.Arcane ? GetGradePower(grade) * 0.35d : 0d;
        }

        public double GetSkillCooldownReductionPercent(RuneGrade grade)
        {
            return EffectKind == RuneEffectKind.Arcane ? Math.Min(4d, GetGradePower(grade) * 0.08d) : 0d;
        }

        public double GetCriticalChancePercent(RuneGrade grade)
        {
            return EffectKind == RuneEffectKind.Focus ? Math.Min(2d, GetGradePower(grade) * 0.10d) : 0d;
        }

        public int GetLevelUpCost(int level)
        {
            if (level >= MaxLevel)
            {
                return 0;
            }

            double cost = Math.Ceiling(10d * Math.Pow(Mathf.Max(1, level), 1.20d));
            if (double.IsNaN(cost) || cost <= 1d)
            {
                return 1;
            }

            return cost >= GameData.MaxIntBalanceValue
                ? GameData.MaxIntBalanceValue
                : Mathf.Max(1, (int)cost);
        }

    }

}
