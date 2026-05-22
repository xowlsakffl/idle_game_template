using System;
using System.Collections.Generic;
using UnityEngine;

public enum TotemArchetype
{
    Combat,
    Guardian,
    Support,
    Arcane,
    Storm,
    Command
}

public enum TotemGrade
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}

[Serializable]
public sealed class TotemDefinition
{
    public const int MaxLevel = 100;

    public TotemDefinition(
        string id,
        string displayName,
        string icon,
        string role,
        TotemArchetype archetype,
        bool startUnlocked = true)
    {
        Id = id;
        DisplayName = displayName;
        Icon = icon;
        Role = role;
        Archetype = archetype;
        StartUnlocked = startUnlocked;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string Icon { get; }
    public string Role { get; }
    public TotemArchetype Archetype { get; }
    public bool StartUnlocked { get; }

    public int GetLevelUpCost(int level, TotemGrade grade)
    {
        if (level >= MaxLevel)
        {
            return 0;
        }

        double cost = Math.Ceiling(6d * GetGradeCostMultiplier(grade) * Math.Pow(Mathf.Max(1, level), 1.22d));
        if (double.IsNaN(cost) || cost <= 1d)
        {
            return 1;
        }

        return cost >= GameData.MaxIntBalanceValue
            ? GameData.MaxIntBalanceValue
            : Mathf.Max(1, (int)cost);
    }

    public int GetPromoteCost(TotemGrade grade)
    {
        switch (grade)
        {
            case TotemGrade.Common:
                return 300;
            case TotemGrade.Uncommon:
                return 900;
            case TotemGrade.Rare:
                return 2700;
            case TotemGrade.Epic:
                return 8100;
            case TotemGrade.Legendary:
                return 24000;
            default:
                return 0;
        }
    }

    public string GetDisplayName(TotemGrade grade)
    {
        return GetGradePrefix(grade) + " " + DisplayName;
    }

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

    public static TotemGrade GetNextGrade(TotemGrade grade)
    {
        return grade >= TotemGrade.Mythic ? TotemGrade.Mythic : (TotemGrade)((int)grade + 1);
    }

    public static string GetGradeLabel(TotemGrade grade)
    {
        switch (grade)
        {
            case TotemGrade.Common:
                return "커먼";
            case TotemGrade.Uncommon:
                return "언커먼";
            case TotemGrade.Rare:
                return "레어";
            case TotemGrade.Epic:
                return "에픽";
            case TotemGrade.Legendary:
                return "전설";
            case TotemGrade.Mythic:
                return "신화";
            default:
                return grade.ToString();
        }
    }

    private static string GetGradePrefix(TotemGrade grade)
    {
        switch (grade)
        {
            case TotemGrade.Common:
                return "낡은";
            case TotemGrade.Uncommon:
                return "정제된";
            case TotemGrade.Rare:
                return "용맹의";
            case TotemGrade.Epic:
                return "영웅의";
            case TotemGrade.Legendary:
                return "고대";
            case TotemGrade.Mythic:
                return "신화";
            default:
                return string.Empty;
        }
    }

    private static double GetGradeEffectMultiplier(TotemGrade grade)
    {
        switch (grade)
        {
            case TotemGrade.Common:
                return 1.00d;
            case TotemGrade.Uncommon:
                return 1.25d;
            case TotemGrade.Rare:
                return 1.60d;
            case TotemGrade.Epic:
                return 2.05d;
            case TotemGrade.Legendary:
                return 2.65d;
            case TotemGrade.Mythic:
                return 3.40d;
            default:
                return 1d;
        }
    }

    private static double GetGradeCostMultiplier(TotemGrade grade)
    {
        switch (grade)
        {
            case TotemGrade.Common:
                return 1.00d;
            case TotemGrade.Uncommon:
                return 1.45d;
            case TotemGrade.Rare:
                return 2.15d;
            case TotemGrade.Epic:
                return 3.25d;
            case TotemGrade.Legendary:
                return 5.00d;
            case TotemGrade.Mythic:
                return 8.00d;
            default:
                return 1d;
        }
    }

}

[Serializable]
public sealed class TotemState
{
    public TotemState(TotemDefinition definition, int level, TotemGrade grade, bool unlocked)
    {
        Definition = definition;
        Level = Mathf.Clamp(level, 1, TotemDefinition.MaxLevel);
        Grade = grade;
        Unlocked = unlocked || definition.StartUnlocked;
    }

    public TotemDefinition Definition { get; }
    public int Level { get; set; }
    public TotemGrade Grade { get; set; }
    public bool Unlocked { get; set; }
    public bool IsMaxed => Level >= TotemDefinition.MaxLevel;
    public bool CanPromote => IsMaxed && Grade < TotemGrade.Mythic;
    public string DisplayName => Definition.GetDisplayName(Grade);
    public string GradeLabel => TotemDefinition.GetGradeLabel(Grade);
    public int LevelUpCost => Definition.GetLevelUpCost(Level, Grade);
    public int PromoteCost => Definition.GetPromoteCost(Grade);
}

public enum RuneEffectKind
{
    Strike,
    Execute,
    Barrier,
    Harvest,
    Arcane,
    Storm,
    Focus,
    Vitality,
    Command,
    Regeneration
}

public enum RuneGrade
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}

[Serializable]
public sealed class RuneDefinition
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

[Serializable]
public sealed class RuneState
{
    public RuneState(RuneDefinition definition, RuneGrade grade, int copies, bool unlocked)
    {
        Definition = definition;
        Grade = (RuneGrade)Mathf.Clamp((int)grade, 0, (int)RuneDefinition.MaxGrade);
        Copies = Mathf.Max(0, copies);
        Unlocked = unlocked || definition.StartUnlocked;
    }

    public RuneDefinition Definition { get; }
    public RuneGrade Grade { get; set; }
    public int Copies { get; set; }
    public bool Unlocked { get; set; }
    public int Level => 1 + (int)Grade * 20;
    public bool IsMaxed => IsMaxGrade;
    public int LevelUpCost => PromoteCost;
    public bool IsMaxGrade => Grade >= RuneDefinition.MaxGrade;
    public bool CanPromote => Unlocked && !IsMaxGrade && Copies >= PromoteCost;
    public int PromoteCost => Definition.GetPromoteRequirement(Grade);
    public string GradeLabel => RuneDefinition.GetGradeLabel(Grade);
}
