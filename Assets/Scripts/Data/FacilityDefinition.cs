using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum FacilityRewardKind
{
    Gold,
    HeroExpItem,
    EquipmentExpItem,
    TotemEssence,
    RuneCopyBox,
    HeroTranscendStone
}

[Serializable]
public readonly struct FacilityUpgradeCost
{
    public FacilityUpgradeCost(long wood, long brick, long iron)
    {
        Wood = Math.Max(0L, wood);
        Brick = Math.Max(0L, brick);
        Iron = Math.Max(0L, iron);
    }

    public long Wood { get; }
    public long Brick { get; }
    public long Iron { get; }
    public bool IsFree => Wood <= 0L && Brick <= 0L && Iron <= 0L;

    public bool CanAfford(CurrencyWallet wallet)
    {
        return wallet != null
            && wallet.Wood >= Wood
            && wallet.Brick >= Brick
            && wallet.Iron >= Iron;
    }

    public string Format()
    {
        var parts = new List<string>();
        if (Wood > 0L)
        {
            parts.Add("목재 " + Wood);
        }

        if (Brick > 0L)
        {
            parts.Add("벽돌 " + Brick);
        }

        if (Iron > 0L)
        {
            parts.Add("철재 " + Iron);
        }

        return parts.Count > 0 ? string.Join(" / ", parts) : "-";
    }
}

[Serializable]
public sealed class FacilityDefinition
{
    public const int MaxLevel = 20;
    public const int MaxAssignedHeroSlots = 5;
    public const double ProductionCycleSeconds = 3600d;
    public const double MaxAccumulatedSeconds = 43200d;
    public const double LevelBonusPerLevel = 0.08d;
    public const double MaxHeroProductionBonusPercent = 50d;

    public FacilityDefinition(string id, string displayName, string icon, FacilityRewardKind rewardKind, string rewardLabel, GameNumber baseProductionPerHour)
    {
        Id = id;
        DisplayName = displayName;
        Icon = icon ?? string.Empty;
        RewardKind = rewardKind;
        RewardLabel = rewardLabel ?? string.Empty;
        BaseProductionPerHour = baseProductionPerHour;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string Icon { get; }
    public FacilityRewardKind RewardKind { get; }
    public string RewardLabel { get; }
    public GameNumber BaseProductionPerHour { get; }

    public int GetUnlockedSlotCount(int level)
    {
        int clampedLevel = Mathf.Clamp(level, 1, MaxLevel);
        if (clampedLevel >= 20)
        {
            return 5;
        }

        if (clampedLevel >= 15)
        {
            return 4;
        }

        if (clampedLevel >= 10)
        {
            return 3;
        }

        if (clampedLevel >= 5)
        {
            return 2;
        }

        return 1;
    }

    public double GetLevelMultiplier(int level)
    {
        return 1d + (Mathf.Clamp(level, 1, MaxLevel) - 1) * LevelBonusPerLevel;
    }

    public GameNumber GetProductionPerHour(int level, double heroBonusPercent)
    {
        double bonus = Mathf.Clamp((float)heroBonusPercent, 0f, (float)MaxHeroProductionBonusPercent);
        return GameNumber.Floor(BaseProductionPerHour * GetLevelMultiplier(level) * (1d + bonus / 100d));
    }

    public FacilityUpgradeCost GetUpgradeCost(int currentLevel)
    {
        if (currentLevel >= MaxLevel)
        {
            return new FacilityUpgradeCost(0, 0, 0);
        }

        int targetLevel = Mathf.Clamp(currentLevel + 1, 2, MaxLevel);
        long baseCost = Math.Max(1L, (long)Math.Ceiling(20d * Math.Pow(targetLevel, 1.35d)));
        if (targetLevel <= 5)
        {
            return new FacilityUpgradeCost(baseCost, 0, 0);
        }

        if (targetLevel <= 10)
        {
            return new FacilityUpgradeCost(baseCost, (long)Math.Ceiling(baseCost * 0.45d), 0);
        }

        if (targetLevel <= 15)
        {
            return new FacilityUpgradeCost(0, baseCost, (long)Math.Ceiling(baseCost * 0.35d));
        }

        return new FacilityUpgradeCost(
            (long)Math.Ceiling(baseCost * 0.70d),
            baseCost,
            (long)Math.Ceiling(baseCost * 0.60d));
    }
}

[Serializable]
public sealed class FacilityState
{
    private readonly List<string> assignedHeroIds = new List<string>(FacilityDefinition.MaxAssignedHeroSlots);

    public FacilityState(FacilityDefinition definition, int level, GameNumber storedAmount, long lastUpdateUtcTicks)
    {
        Definition = definition;
        Level = Mathf.Clamp(level, 1, FacilityDefinition.MaxLevel);
        StoredAmount = GameNumber.Max(GameNumber.Zero, storedAmount);
        LastUpdateUtcTicks = Math.Max(0L, lastUpdateUtcTicks);
        for (int i = 0; i < FacilityDefinition.MaxAssignedHeroSlots; i++)
        {
            assignedHeroIds.Add(string.Empty);
        }
    }

    public FacilityDefinition Definition { get; }
    public int Level { get; set; }
    public GameNumber StoredAmount { get; set; }
    public long LastUpdateUtcTicks { get; set; }
    public IReadOnlyList<string> AssignedHeroIds => assignedHeroIds;
    public bool IsMaxed => Level >= FacilityDefinition.MaxLevel;
    public int UnlockedSlotCount => Definition.GetUnlockedSlotCount(Level);
    public FacilityUpgradeCost UpgradeCost => Definition.GetUpgradeCost(Level);

    public string GetAssignedHeroId(int slot)
    {
        return slot >= 0 && slot < assignedHeroIds.Count ? assignedHeroIds[slot] : string.Empty;
    }

    public void SetAssignedHeroId(int slot, string heroId)
    {
        if (slot < 0 || slot >= assignedHeroIds.Count)
        {
            return;
        }

        assignedHeroIds[slot] = heroId ?? string.Empty;
    }

    public void ClearAssignments()
    {
        for (int i = 0; i < assignedHeroIds.Count; i++)
        {
            assignedHeroIds[i] = string.Empty;
        }
    }

    public int AssignedCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < UnlockedSlotCount; i++)
            {
                if (!string.IsNullOrEmpty(assignedHeroIds[i]))
                {
                    count += 1;
                }
            }

            return count;
        }
    }
}
