using System;
using UnityEngine;

[Serializable]
public sealed class CombatSkillDefinition
{
    public CombatSkillDefinition(string id, string displayName, float cooldownSeconds, float partyAttackMultiplier)
    {
        Id = id;
        DisplayName = displayName;
        CooldownSeconds = Mathf.Max(0.1f, cooldownSeconds);
        PartyAttackMultiplier = Mathf.Max(0.1f, partyAttackMultiplier);
    }

    public string Id { get; }
    public string DisplayName { get; }
    public float CooldownSeconds { get; }
    public float PartyAttackMultiplier { get; }
}

public sealed class CombatSkillState
{
    public CombatSkillState(CombatSkillDefinition definition)
    {
        Definition = definition;
        CooldownRemaining = definition.CooldownSeconds;
    }

    public CombatSkillDefinition Definition { get; }
    public float CooldownRemaining { get; set; }
}
