using System;
using UnityEngine;

public enum AbilityKind
{
    AttackPower,
    CriticalChance,
    CriticalDamage
}

[Serializable]
public sealed class AbilityDefinition
{
    public AbilityDefinition(
        AbilityKind kind,
        string displayName,
        string description,
        int valuePerLevel,
        int baseValue,
        int maxLevel,
        int baseCost,
        float costGrowth)
    {
        Kind = kind;
        DisplayName = displayName;
        Description = description;
        ValuePerLevel = valuePerLevel;
        BaseValue = baseValue;
        MaxLevel = maxLevel;
        BaseCost = baseCost;
        CostGrowth = costGrowth;
    }

    public AbilityKind Kind { get; }
    public string Id => Kind.ToString();
    public string DisplayName { get; }
    public string Description { get; }
    public int ValuePerLevel { get; }
    public int BaseValue { get; }
    public int MaxLevel { get; }
    public int BaseCost { get; }
    public float CostGrowth { get; }

    public int GetRawValue(int level)
    {
        return BaseValue + Mathf.Max(0, level) * ValuePerLevel;
    }

    public long GetLevelUpCost(int level)
    {
        if (MaxLevel > 0 && level >= MaxLevel)
        {
            return 0;
        }

        int nextLevel = Mathf.Max(1, level + 1);
        return Mathf.FloorToInt(BaseCost * Mathf.Pow(nextLevel, CostGrowth));
    }
}

public sealed class AbilityState
{
    public AbilityState(AbilityDefinition definition, int level)
    {
        Definition = definition;
        Level = Mathf.Max(0, level);
    }

    public AbilityDefinition Definition { get; }
    public int Level { get; set; }
    public bool IsMaxed => Definition.MaxLevel > 0 && Level >= Definition.MaxLevel;
    public int RawValue => Definition.GetRawValue(Level);
    public long LevelUpCost => Definition.GetLevelUpCost(Level);
}
