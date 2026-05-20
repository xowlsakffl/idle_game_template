using System;
using UnityEngine;

public enum AbilityKind
{
    AttackPower,
    MaxHp,
    CriticalChance,
    CriticalDamage,
    DoubleCriticalChance,
    DoubleCriticalBonusDamage,
    FinalDamage
}

public enum AbilityDisplayKind
{
    Flat,
    Percent
}

[Serializable]
public sealed class AbilityDefinition
{
    public AbilityDefinition(
        AbilityKind kind,
        string displayName,
        string description,
        double valuePerLevel,
        double baseValue,
        int maxLevel,
        long baseCost,
        float costGrowth,
        AbilityDisplayKind displayKind)
    {
        Kind = kind;
        DisplayName = displayName;
        Description = description;
        ValuePerLevel = valuePerLevel;
        BaseValue = baseValue;
        MaxLevel = maxLevel;
        BaseCost = baseCost;
        CostGrowth = costGrowth;
        DisplayKind = displayKind;
    }

    public AbilityKind Kind { get; }
    public string Id => Kind.ToString();
    public string DisplayName { get; }
    public string Description { get; }
    public double ValuePerLevel { get; }
    public double BaseValue { get; }
    public int MaxLevel { get; }
    public long BaseCost { get; }
    public float CostGrowth { get; }
    public AbilityDisplayKind DisplayKind { get; }

    public double GetValue(int level)
    {
        return BaseValue + Math.Max(0, level) * ValuePerLevel;
    }

    public long GetLevelUpCost(int level)
    {
        if (MaxLevel > 0 && level >= MaxLevel)
        {
            return 0;
        }

        int nextLevel = Mathf.Max(1, level + 1);
        double cost = BaseCost
            * GetBalanceCostMultiplier(Kind)
            * Math.Pow(nextLevel, CostGrowth + GetBalanceGrowthBonus(Kind));
        if (double.IsNaN(cost) || cost <= 0d)
        {
            return 1;
        }

        if (cost >= long.MaxValue)
        {
            return long.MaxValue;
        }

        return Math.Max(1L, (long)Math.Floor(cost));
    }

    public string FormatValue(int level)
    {
        double value = GetValue(level);
        return DisplayKind == AbilityDisplayKind.Percent
            ? value.ToString("0.#") + "%"
            : FormatShortNumber(value);
    }

    public static string FormatShortNumber(double value)
    {
        return NumberFormatter.Format(value);
    }

    private static double GetBalanceCostMultiplier(AbilityKind kind)
    {
        switch (kind)
        {
            case AbilityKind.CriticalChance:
                return 3.0d;
            case AbilityKind.DoubleCriticalChance:
                return 3.5d;
            case AbilityKind.FinalDamage:
                return 2.4d;
            case AbilityKind.CriticalDamage:
            case AbilityKind.DoubleCriticalBonusDamage:
                return 2.0d;
            case AbilityKind.AttackPower:
            case AbilityKind.MaxHp:
                return 1.8d;
            default:
                return 1d;
        }
    }

    private static double GetBalanceGrowthBonus(AbilityKind kind)
    {
        switch (kind)
        {
            case AbilityKind.CriticalChance:
            case AbilityKind.DoubleCriticalChance:
                return 0.08d;
            case AbilityKind.FinalDamage:
                return 0.06d;
            case AbilityKind.CriticalDamage:
            case AbilityKind.DoubleCriticalBonusDamage:
                return 0.04d;
            case AbilityKind.AttackPower:
            case AbilityKind.MaxHp:
                return 0.03d;
            default:
                return 0d;
        }
    }
}

public sealed class AbilityState
{
    public AbilityState(AbilityDefinition definition, int level)
    {
        Definition = definition;
        Level = definition.MaxLevel > 0
            ? Mathf.Clamp(level, 0, definition.MaxLevel)
            : Mathf.Max(0, level);
    }

    public AbilityDefinition Definition { get; }
    public int Level { get; set; }
    public bool IsMaxed => Definition.MaxLevel > 0 && Level >= Definition.MaxLevel;
    public double Value => Definition.GetValue(Level);
    public long LevelUpCost => Definition.GetLevelUpCost(Level);
}
