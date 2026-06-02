using System.Collections.Generic;
using System;
using UnityEngine;

namespace IdleGame.Data
{
    public sealed partial class TotemDefinition
    {
        public string GetEffectSummary(int level, TotemGrade grade, IReadOnlyList<HeroState> formationHeroes = null, bool boss = false)
        {
            int effectiveLevel = Mathf.Clamp(level, 1, MaxLevel);
            switch (Archetype)
            {
                case TotemArchetype.Combat:
                    return "공격력 +" + GetAttackPercent(effectiveLevel, grade, formationHeroes, false).ToString("0.##") + "%"
                        + "\n치명타 확률 +" + GetCriticalChancePercent(effectiveLevel, grade).ToString("0.##") + "%"
                        + "\n보스전 공격력 +" + GetBossAttackPercent(effectiveLevel, grade).ToString("0.##") + "%";
                case TotemArchetype.Guardian:
                    return "체력 +" + GetHpPercent(effectiveLevel, grade, formationHeroes).ToString("0.##") + "%"
                        + "\n받는 피해 -" + GetDamageReductionPercent(effectiveLevel, grade).ToString("0.##") + "%"
                        + "\n방어형 영웅 체력 +" + GetTraitHpPercent(effectiveLevel, grade, HeroTrait.Defense).ToString("0.##") + "%";
                case TotemArchetype.Support:
                    return "골드 +" + GetGoldGainPercent(effectiveLevel, grade).ToString("0.##") + "%"
                        + "\n경험치책 +" + GetHeroExpGainPercent(effectiveLevel, grade).ToString("0.##") + "%"
                        + "\n계정 경험치 +" + GetAccountExpGainPercent(effectiveLevel, grade).ToString("0.##") + "%";
                case TotemArchetype.Arcane:
                    return "스킬 피해 +" + GetSkillDamagePercent(effectiveLevel, grade).ToString("0.##") + "%"
                        + "\n스킬 쿨타임 -" + GetSkillCooldownReductionPercent(effectiveLevel, grade).ToString("0.##") + "%"
                        + "\n자동 스킬 안정화 보너스";
                case TotemArchetype.Storm:
                    return "공속 +" + GetAttackSpeedPercent(effectiveLevel, grade, null).ToString("0.##") + "%"
                        + "\n이속 +" + GetMoveSpeedPercent(effectiveLevel, grade).ToString("0.##") + "%"
                        + "\n원거리 영웅 추가 공속 +" + GetTraitAttackSpeedBonusPercent(effectiveLevel, grade, HeroTrait.Ranged).ToString("0.##") + "%";
                case TotemArchetype.Command:
                    return "파티 공격력 +" + GetAttackPercent(effectiveLevel, grade, formationHeroes, false).ToString("0.##") + "%"
                        + "\n파티 체력 +" + GetHpPercent(effectiveLevel, grade, formationHeroes).ToString("0.##") + "%"
                        + "\n스킬 피해 +" + GetSkillDamagePercent(effectiveLevel, grade).ToString("0.##") + "%";
                default:
                    return string.Empty;
            }
        }

        public double GetAttackPercent(int level, TotemGrade grade, IReadOnlyList<HeroState> formationHeroes, bool boss)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            switch (Archetype)
            {
                case TotemArchetype.Combat:
                    return level * 0.025d * GetGradeEffectMultiplier(grade) + (boss ? GetBossAttackPercent(level, grade) : 0d);
                case TotemArchetype.Command:
                    return level * 0.012d * GetGradeEffectMultiplier(grade);
                default:
                    return 0d;
            }
        }

        public double GetBossAttackPercent(int level, TotemGrade grade)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return Archetype == TotemArchetype.Combat && level >= 25
                ? (0.6d + (int)grade * 0.25d)
                : 0d;
        }

        public double GetCriticalChancePercent(int level, TotemGrade grade)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return Archetype == TotemArchetype.Combat
                ? Math.Min(3d, level * 0.006d * GetGradeEffectMultiplier(grade))
                : 0d;
        }

        public double GetTraitAttackPercent(int level, TotemGrade grade, HeroTrait trait, IReadOnlyList<HeroState> formationHeroes)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            switch (Archetype)
            {
                case TotemArchetype.Storm:
                    return level >= 25 && trait == HeroTrait.Ranged ? 0.4d + (int)grade * 0.18d : 0d;
                default:
                    return 0d;
            }
        }

        public double GetHpPercent(int level, TotemGrade grade, IReadOnlyList<HeroState> formationHeroes)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            switch (Archetype)
            {
                case TotemArchetype.Guardian:
                    return level * 0.035d * GetGradeEffectMultiplier(grade);
                case TotemArchetype.Command:
                    return level * 0.012d * GetGradeEffectMultiplier(grade);
                default:
                    return 0d;
            }
        }

        public double GetTraitHpPercent(int level, TotemGrade grade, HeroTrait trait)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return Archetype == TotemArchetype.Guardian && level >= 25 && trait == HeroTrait.Defense
                ? 0.7d + (int)grade * 0.25d
                : 0d;
        }

        public double GetGoldGainPercent(int level, TotemGrade grade)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return Archetype == TotemArchetype.Support
                ? level * 0.03d * GetGradeEffectMultiplier(grade)
                : 0d;
        }

        public double GetHeroExpGainPercent(int level, TotemGrade grade)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return Archetype == TotemArchetype.Support
                ? level * 0.02d * GetGradeEffectMultiplier(grade) + (level >= 50 ? 0.5d + (int)grade * 0.15d : 0d)
                : 0d;
        }

        public double GetAccountExpGainPercent(int level, TotemGrade grade)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return Archetype == TotemArchetype.Support && level >= 75
                ? 0.7d + (int)grade * 0.2d
                : 0d;
        }

        public double GetDamageReductionPercent(int level, TotemGrade grade)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return Archetype == TotemArchetype.Guardian
                ? Math.Min(8d, level * 0.008d * GetGradeEffectMultiplier(grade) + (level >= 50 ? 0.4d : 0d))
                : 0d;
        }

        public double GetAttackSpeedPercent(int level, TotemGrade grade, HeroState hero)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            if (Archetype != TotemArchetype.Storm)
            {
                return 0d;
            }

            double bonus = level * 0.018d * GetGradeEffectMultiplier(grade);
            if (level >= 25 && hero != null && hero.Definition.Trait == HeroTrait.Ranged)
            {
                bonus += GetTraitAttackSpeedBonusPercent(level, grade, hero.Definition.Trait);
            }

            return bonus;
        }

        private double GetTraitAttackSpeedBonusPercent(int level, TotemGrade grade, HeroTrait trait)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return level >= 25 && trait == HeroTrait.Ranged
                ? 0.4d + (int)grade * 0.18d
                : 0d;
        }

        public double GetMoveSpeedPercent(int level, TotemGrade grade)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return Archetype == TotemArchetype.Storm
                ? level * 0.02d * GetGradeEffectMultiplier(grade)
                : 0d;
        }

        public double GetSkillDamagePercent(int level, TotemGrade grade)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            switch (Archetype)
            {
                case TotemArchetype.Arcane:
                    return level * 0.032d * GetGradeEffectMultiplier(grade);
                case TotemArchetype.Command:
                    return level * 0.010d * GetGradeEffectMultiplier(grade);
                default:
                    return 0d;
            }
        }

        public double GetSkillCooldownReductionPercent(int level, TotemGrade grade)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return Archetype == TotemArchetype.Arcane
                ? Math.Min(10d, level * 0.01d * GetGradeEffectMultiplier(grade))
                : 0d;
        }
    }
}
