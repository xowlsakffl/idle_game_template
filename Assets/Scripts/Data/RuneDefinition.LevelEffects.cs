using System;
using UnityEngine;

namespace IdleGame.Data
{
    public sealed partial class RuneDefinition
    {
        public string GetEffectSummary(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            switch (EffectKind)
            {
                case RuneEffectKind.Strike:
                    return "공격력 +" + GetAttackPercent(level).ToString("0.##") + "%";
                case RuneEffectKind.Execute:
                    return "최종 피해 +" + GetFinalDamagePercent(level).ToString("0.##") + "%";
                case RuneEffectKind.Barrier:
                    return "체력 +" + GetHpPercent(level).ToString("0.##") + "%\n받는 피해 -" + GetDamageReductionPercent(level).ToString("0.##") + "%";
                case RuneEffectKind.Harvest:
                    return "골드 +" + GetGoldGainPercent(level).ToString("0.##") + "%\n경험치책 +" + GetHeroExpGainPercent(level).ToString("0.##") + "%";
                case RuneEffectKind.Arcane:
                    return "스킬 피해 +" + GetSkillDamagePercent(level).ToString("0.##") + "%\n스킬 쿨타임 -" + GetSkillCooldownReductionPercent(level).ToString("0.##") + "%";
                case RuneEffectKind.Storm:
                    return "공속 +" + GetAttackSpeedPercent(level).ToString("0.##") + "%\n이속 +" + GetMoveSpeedPercent(level).ToString("0.##") + "%";
                case RuneEffectKind.Focus:
                    return "치명타 확률 +" + GetCriticalChancePercent(level).ToString("0.##") + "%";
                case RuneEffectKind.Vitality:
                    return "체력 +" + GetHpPercent(level).ToString("0.##") + "%";
                case RuneEffectKind.Command:
                    return "공격력 +" + GetAttackPercent(level).ToString("0.##") + "%\n계정 경험치 +" + GetAccountExpGainPercent(level).ToString("0.##") + "%";
                case RuneEffectKind.Regeneration:
                    return "받는 피해 -" + GetDamageReductionPercent(level).ToString("0.##") + "%";
                default:
                    return Role;
            }
        }

        public double GetAttackPercent(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            switch (EffectKind)
            {
                case RuneEffectKind.Strike:
                    return level * 0.045d;
                case RuneEffectKind.Command:
                    return level * 0.025d;
                default:
                    return 0d;
            }
        }

        public double GetFinalDamagePercent(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return EffectKind == RuneEffectKind.Execute ? level * 0.025d : 0d;
        }

        public double GetHpPercent(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            switch (EffectKind)
            {
                case RuneEffectKind.Barrier:
                    return level * 0.035d;
                case RuneEffectKind.Vitality:
                    return level * 0.055d;
                default:
                    return 0d;
            }
        }

        public double GetDamageReductionPercent(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            switch (EffectKind)
            {
                case RuneEffectKind.Barrier:
                    return level * 0.015d;
                case RuneEffectKind.Regeneration:
                    return level * 0.012d;
                default:
                    return 0d;
            }
        }

        public double GetGoldGainPercent(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return EffectKind == RuneEffectKind.Harvest ? level * 0.050d : 0d;
        }

        public double GetHeroExpGainPercent(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return EffectKind == RuneEffectKind.Harvest ? level * 0.035d : 0d;
        }

        public double GetAccountExpGainPercent(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return EffectKind == RuneEffectKind.Command ? level * 0.035d : 0d;
        }

        public double GetAttackSpeedPercent(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return EffectKind == RuneEffectKind.Storm ? level * 0.030d : 0d;
        }

        public double GetMoveSpeedPercent(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return EffectKind == RuneEffectKind.Storm ? level * 0.025d : 0d;
        }

        public double GetSkillDamagePercent(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return EffectKind == RuneEffectKind.Arcane ? level * 0.045d : 0d;
        }

        public double GetSkillCooldownReductionPercent(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return EffectKind == RuneEffectKind.Arcane ? Math.Min(4d, level * 0.010d) : 0d;
        }

        public double GetCriticalChancePercent(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return EffectKind == RuneEffectKind.Focus ? Math.Min(2d, level * 0.012d) : 0d;
        }
    }
}
