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
        GameNumber hpMultiplier,
        GameNumber goldMultiplier,
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
    public GameNumber HpMultiplier { get; }
    public GameNumber GoldMultiplier { get; }
    public int RequiredKills { get; }
    public string FailureStageId { get; }

    public bool IsBoss => Type == StageType.Boss;
}

[Serializable]
public readonly struct StageClearReward
{
    public StageClearReward(
        int heroSummonTickets,
        int equipmentSummonTickets,
        int ruby,
        int heroExpItems,
        int equipmentExpItems,
        int heroTranscendStones)
    {
        HeroSummonTickets = Mathf.Max(0, heroSummonTickets);
        EquipmentSummonTickets = Mathf.Max(0, equipmentSummonTickets);
        Ruby = Mathf.Max(0, ruby);
        HeroExpItems = Mathf.Max(0, heroExpItems);
        EquipmentExpItems = Mathf.Max(0, equipmentExpItems);
        HeroTranscendStones = Mathf.Max(0, heroTranscendStones);
    }

    public static StageClearReward Empty => new StageClearReward(0, 0, 0, 0, 0, 0);

    public int HeroSummonTickets { get; }
    public int EquipmentSummonTickets { get; }
    public int Ruby { get; }
    public int HeroExpItems { get; }
    public int EquipmentExpItems { get; }
    public int HeroTranscendStones { get; }

    public bool IsEmpty => HeroSummonTickets <= 0
        && EquipmentSummonTickets <= 0
        && Ruby <= 0
        && HeroExpItems <= 0
        && EquipmentExpItems <= 0
        && HeroTranscendStones <= 0;
}
