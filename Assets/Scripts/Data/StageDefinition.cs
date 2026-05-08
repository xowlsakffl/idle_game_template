using System;
using UnityEngine;

public enum StageType
{
    Normal,
    Boss
}

[Serializable]
public sealed class StageDefinition
{
    public StageDefinition(
        string id,
        int chapter,
        int number,
        StageType type,
        string targetId,
        float hpMultiplier,
        float goldMultiplier,
        int requiredKills,
        string failureStageId)
    {
        Id = id;
        Chapter = chapter;
        Number = number;
        Type = type;
        TargetId = targetId;
        HpMultiplier = hpMultiplier;
        GoldMultiplier = goldMultiplier;
        RequiredKills = Mathf.Max(1, requiredKills);
        FailureStageId = failureStageId;
    }

    public string Id { get; }
    public int Chapter { get; }
    public int Number { get; }
    public StageType Type { get; }
    public string TargetId { get; }
    public float HpMultiplier { get; }
    public float GoldMultiplier { get; }
    public int RequiredKills { get; }
    public string FailureStageId { get; }

    public bool IsBoss => Type == StageType.Boss;
}
