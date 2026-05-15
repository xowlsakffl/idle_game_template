using System;
using UnityEngine;

[Serializable]
public sealed class PetDefinition
{
    public PetDefinition(string id, string displayName, int attackPower, float attackInterval, float goldBonusPercent)
    {
        Id = id;
        DisplayName = displayName;
        AttackPower = Mathf.Max(1, attackPower);
        AttackInterval = Mathf.Max(0.1f, attackInterval);
        GoldBonusPercent = Mathf.Max(0f, goldBonusPercent);
    }

    public string Id { get; }
    public string DisplayName { get; }
    public int AttackPower { get; }
    public float AttackInterval { get; }
    public float GoldBonusPercent { get; }
}

public sealed class PetState
{
    public PetState(PetDefinition definition)
    {
        Definition = definition;
        AttackCooldown = definition.AttackInterval;
    }

    public PetDefinition Definition { get; }
    public float AttackCooldown { get; set; }
}
