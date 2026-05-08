using System;
using UnityEngine;

[Serializable]
public sealed class HeroDefinition
{
    public HeroDefinition(string id, string displayName, string role, int baseAttack, float attackInterval, int attackPerLevel)
    {
        Id = id;
        DisplayName = displayName;
        Role = role;
        BaseAttack = baseAttack;
        AttackInterval = attackInterval;
        AttackPerLevel = attackPerLevel;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string Role { get; }
    public int BaseAttack { get; }
    public float AttackInterval { get; }
    public int AttackPerLevel { get; }

    public int GetAttackPower(int level)
    {
        return BaseAttack + Mathf.Max(0, level - 1) * AttackPerLevel;
    }

    public int GetLevelUpCost(int level)
    {
        return Mathf.FloorToInt(20f * Mathf.Pow(Mathf.Max(1, level), 1.25f));
    }
}

public sealed class HeroState
{
    public HeroState(HeroDefinition definition, int level, int shards)
    {
        Definition = definition;
        Level = Mathf.Max(1, level);
        Shards = Mathf.Max(0, shards);
    }

    public HeroDefinition Definition { get; }
    public int Level { get; set; }
    public int Shards { get; set; }
    public float AttackCooldown { get; set; }

    public int AttackPower => Definition.GetAttackPower(Level);
    public int LevelUpCost => Definition.GetLevelUpCost(Level);
}
