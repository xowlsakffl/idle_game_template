using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum HeroRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}

[Serializable]
public enum HeroTrait
{
    Melee,
    Ranged,
    Support,
    Defense
}

[Serializable]
public enum HeroPassiveStat
{
    AttackPower,
    MaxHp,
    AttackSpeed,
    MoveSpeed
}

[Serializable]
public enum HeroTranscendGrade
{
    F,
    E,
    D,
    C,
    B,
    A,
    S,
    SS
}

[Serializable]
public enum HeroTranscendOptionScope
{
    Common,
    Exclusive
}

[Serializable]
public sealed class HeroTranscendOptionDefinition
{
    public HeroTranscendOptionDefinition(
        string id,
        string heroId,
        HeroTranscendOptionScope scope,
        HeroTranscendGrade grade,
        string description,
        float probabilityWeight)
    {
        Id = id;
        HeroId = heroId ?? string.Empty;
        Scope = scope;
        Grade = grade;
        Description = description ?? string.Empty;
        ProbabilityWeight = Mathf.Max(0.0001f, probabilityWeight);
    }

    public string Id { get; }
    public string HeroId { get; }
    public HeroTranscendOptionScope Scope { get; }
    public HeroTranscendGrade Grade { get; }
    public string Description { get; }
    public float ProbabilityWeight { get; }
    public bool IsExclusive => Scope == HeroTranscendOptionScope.Exclusive;
    public string ScopeLabel => IsExclusive ? "전용" : "공용";
}

[Serializable]
public sealed class HeroTranscendOptionState
{
    public HeroTranscendOptionState(string optionId)
    {
        OptionId = optionId ?? string.Empty;
    }

    public string OptionId { get; set; }
}

[Serializable]
public sealed class HeroDefinition
{
    public const int MaxStars = 15;
    public const int MaxTranscendSlots = 5;
    public const int LevelPerStar = 50;
    public const int MaxLevelAtMaxStars = (MaxStars + 1) * LevelPerStar;
    private const float FiveStarPassiveBoostMultiplier = 1.5f;
    private const float TenStarAllStatMultiplier = 1.1f;
    private static readonly int[] TranscendSlotRequiredStars = { 0, 1, 3, 7, 11 };

    public HeroDefinition(
        string id,
        string displayName,
        string role,
        HeroRarity rarity,
        HeroTrait trait,
        int baseAttack,
        int baseHp,
        float attackSpeed,
        float moveSpeed,
        int attackPerLevel,
        int hpPerLevel,
        HeroPassiveStat passiveStat,
        float passiveValuePercent,
        bool startUnlocked = false)
    {
        Id = id;
        DisplayName = displayName;
        Role = role;
        Rarity = rarity;
        Trait = trait;
        BaseAttack = baseAttack;
        BaseHp = baseHp;
        AttackSpeed = Mathf.Max(0.1f, attackSpeed);
        MoveSpeed = Mathf.Max(0.1f, moveSpeed);
        AttackPerLevel = attackPerLevel;
        HpPerLevel = hpPerLevel;
        PassiveStat = passiveStat;
        PassiveValuePercent = GetBalancedPassiveValue(rarity, passiveValuePercent);
        StartUnlocked = startUnlocked;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string Role { get; }
    public HeroRarity Rarity { get; }
    public HeroTrait Trait { get; }
    public int BaseAttack { get; }
    public int BaseHp { get; }
    public float AttackSpeed { get; }
    public float MoveSpeed { get; }
    public float AttackInterval => 1f / AttackSpeed;
    public int AttackPerLevel { get; }
    public int HpPerLevel { get; }
    public HeroPassiveStat PassiveStat { get; }
    public float PassiveValuePercent { get; }
    public bool StartUnlocked { get; }
    public string RarityLabel => GetRarityLabel(Rarity);
    public string PassiveLabel => GetPassiveLabel(PassiveStat) + " +" + PassiveValuePercent.ToString("0.#") + "%";

    public int GetAttackPower(int level)
    {
        return GetAttackPower(level, 0);
    }

    public int GetAttackPower(int level, int stars)
    {
        int levelAttack = BaseAttack + Mathf.Max(0, level - 1) * AttackPerLevel;
        float starMultiplier = 1f + Mathf.Clamp(stars, 0, MaxStars) * 0.06f;
        return Mathf.Clamp(Mathf.FloorToInt(levelAttack
            * starMultiplier
            * GetRarityStatMultiplier(Rarity)
            * GetPassiveMultiplier(HeroPassiveStat.AttackPower, stars)
            * GetTenStarMultiplier(stars)), 1, GameData.MaxIntBalanceValue);
    }

    public int GetMaxHp(int level, int stars)
    {
        int levelHp = BaseHp + Mathf.Max(0, level - 1) * HpPerLevel;
        float starMultiplier = 1f + Mathf.Clamp(stars, 0, MaxStars) * 0.055f;
        return Mathf.Clamp(Mathf.FloorToInt(levelHp
            * starMultiplier
            * GetRarityStatMultiplier(Rarity)
            * GetPassiveMultiplier(HeroPassiveStat.MaxHp, stars)
            * GetTenStarMultiplier(stars)), 1, GameData.MaxIntBalanceValue);
    }

    public float GetAttackSpeed(int stars)
    {
        return AttackSpeed
            * GetRaritySpeedMultiplier(Rarity)
            * GetPassiveMultiplier(HeroPassiveStat.AttackSpeed, stars)
            * GetTenStarMultiplier(stars);
    }

    public float GetMoveSpeed(int stars)
    {
        return MoveSpeed
            * GetRarityMoveMultiplier(Rarity)
            * GetPassiveMultiplier(HeroPassiveStat.MoveSpeed, stars)
            * GetTenStarMultiplier(stars);
    }

    public int GetLevelUpCost(int level)
    {
        double cost = 18d * Math.Pow(Mathf.Max(1, level), 1.22d) * GetRarityLevelCostMultiplier(Rarity);
        if (double.IsNaN(cost) || cost <= 1d)
        {
            return 1;
        }

        if (cost >= GameData.MaxIntBalanceValue)
        {
            return GameData.MaxIntBalanceValue;
        }

        return Math.Max(1, (int)Math.Floor(cost));
    }

    public int GetMaxLevel(int stars)
    {
        int effectiveStars = Mathf.Clamp(stars, 0, MaxStars) + 1;
        return effectiveStars * LevelPerStar;
    }

    public int GetStarUpCost(int currentStars)
    {
        if (currentStars >= MaxStars)
        {
            return int.MaxValue;
        }

        return GetBaseShardStep(Rarity) * (Mathf.Clamp(currentStars, 0, MaxStars - 1) + 1);
    }

    public int GetSummonShardReward()
    {
        return 1;
    }

    public static int GetTranscendRequiredStars(int slotIndex)
    {
        int clampedIndex = Mathf.Clamp(slotIndex, 0, TranscendSlotRequiredStars.Length - 1);
        return TranscendSlotRequiredStars[clampedIndex];
    }

    private float GetPassiveMultiplier(HeroPassiveStat stat, int stars)
    {
        if (PassiveStat != stat)
        {
            return 1f;
        }

        float passivePercent = PassiveValuePercent;
        if (stars >= 5)
        {
            passivePercent *= FiveStarPassiveBoostMultiplier;
        }

        return 1f + passivePercent / 100f;
    }

    private static float GetTenStarMultiplier(int stars)
    {
        return stars >= 10 ? TenStarAllStatMultiplier : 1f;
    }

    private static float GetBalancedPassiveValue(HeroRarity rarity, float rawValue)
    {
        float scaledValue = Mathf.Max(0f, rawValue) * 0.25f;
        switch (rarity)
        {
            case HeroRarity.Common:
                return Mathf.Clamp(scaledValue, 0.5f, 1.5f);
            case HeroRarity.Uncommon:
                return Mathf.Clamp(scaledValue, 0.8f, 2.2f);
            case HeroRarity.Rare:
                return Mathf.Clamp(scaledValue, 1.2f, 3.0f);
            case HeroRarity.Epic:
                return Mathf.Clamp(scaledValue, 1.8f, 4.0f);
            case HeroRarity.Legendary:
                return Mathf.Clamp(scaledValue, 2.5f, 4.8f);
            case HeroRarity.Mythic:
                return Mathf.Clamp(scaledValue, 3.2f, 5.8f);
            default:
                return scaledValue;
        }
    }

    private static float GetRarityStatMultiplier(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Common:
                return 1.00f;
            case HeroRarity.Uncommon:
                return 1.06f;
            case HeroRarity.Rare:
                return 1.14f;
            case HeroRarity.Epic:
                return 1.26f;
            case HeroRarity.Legendary:
                return 1.42f;
            case HeroRarity.Mythic:
                return 1.62f;
            default:
                return 1f;
        }
    }

    private static float GetRaritySpeedMultiplier(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Common:
                return 1.00f;
            case HeroRarity.Uncommon:
                return 1.025f;
            case HeroRarity.Rare:
                return 1.055f;
            case HeroRarity.Epic:
                return 1.09f;
            case HeroRarity.Legendary:
                return 1.13f;
            case HeroRarity.Mythic:
                return 1.18f;
            default:
                return 1f;
        }
    }

    private static float GetRarityMoveMultiplier(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Common:
                return 1.00f;
            case HeroRarity.Uncommon:
                return 1.015f;
            case HeroRarity.Rare:
                return 1.035f;
            case HeroRarity.Epic:
                return 1.06f;
            case HeroRarity.Legendary:
                return 1.09f;
            case HeroRarity.Mythic:
                return 1.12f;
            default:
                return 1f;
        }
    }

    private static int GetBaseShardStep(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Common:
                return 5;
            case HeroRarity.Uncommon:
                return 10;
            case HeroRarity.Rare:
                return 15;
            case HeroRarity.Epic:
                return 25;
            case HeroRarity.Legendary:
                return 40;
            case HeroRarity.Mythic:
                return 60;
            default:
                return 10;
        }
    }

    private static double GetRarityLevelCostMultiplier(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Common:
                return 1.00d;
            case HeroRarity.Uncommon:
                return 1.25d;
            case HeroRarity.Rare:
                return 1.65d;
            case HeroRarity.Epic:
                return 2.25d;
            case HeroRarity.Legendary:
                return 3.05d;
            case HeroRarity.Mythic:
                return 4.10d;
            default:
                return 1.00d;
        }
    }

    private static string GetRarityLabel(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Common:
                return "커먼";
            case HeroRarity.Uncommon:
                return "언커먼";
            case HeroRarity.Rare:
                return "레어";
            case HeroRarity.Epic:
                return "에픽";
            case HeroRarity.Legendary:
                return "전설";
            case HeroRarity.Mythic:
                return "신화";
            default:
                return rarity.ToString();
        }
    }

    private static string GetPassiveLabel(HeroPassiveStat stat)
    {
        switch (stat)
        {
            case HeroPassiveStat.AttackPower:
                return "공격력";
            case HeroPassiveStat.MaxHp:
                return "체력";
            case HeroPassiveStat.AttackSpeed:
                return "공속";
            case HeroPassiveStat.MoveSpeed:
                return "이속";
            default:
                return stat.ToString();
        }
    }

}

public sealed class HeroState
{
    private readonly List<HeroTranscendOptionState> transcendOptions = new List<HeroTranscendOptionState>(HeroDefinition.MaxTranscendSlots);

    public HeroState(HeroDefinition definition, int level, int shards, int stars)
    {
        Definition = definition;
        Shards = Mathf.Max(0, shards);
        Stars = Mathf.Clamp(stars, 0, HeroDefinition.MaxStars);
        Level = Mathf.Clamp(level, 1, Definition.GetMaxLevel(Stars));

        for (int i = 0; i < HeroDefinition.MaxTranscendSlots; i++)
        {
            transcendOptions.Add(new HeroTranscendOptionState(string.Empty));
        }
    }

    public HeroDefinition Definition { get; }
    public int Level { get; set; }
    public int Shards { get; set; }
    public int Stars { get; set; }
    public float AttackCooldown { get; set; }
    public IReadOnlyList<HeroTranscendOptionState> TranscendOptions => transcendOptions;

    public int AttackPower => Definition.GetAttackPower(Level, Stars);
    public int MaxHp => Definition.GetMaxHp(Level, Stars);
    public float AttackSpeed => Definition.GetAttackSpeed(Stars);
    public float MoveSpeed => Definition.GetMoveSpeed(Stars);
    public float AttackInterval => Mathf.Max(0.1f, 1f / AttackSpeed);
    public int LevelUpCost => Definition.GetLevelUpCost(Level);
    public int MaxLevel => Definition.GetMaxLevel(Stars);
    public bool IsMaxStars => Stars >= HeroDefinition.MaxStars;
    public int StarUpCost => Definition.GetStarUpCost(Stars);
    public bool CanStarUp => !IsMaxStars && Shards >= StarUpCost;
    public bool IsOwned => Definition.StartUnlocked || Shards > 0 || Stars > 0 || Level > 1;

    public bool IsTranscendSlotUnlocked(int slotIndex)
    {
        return IsOwned
            && slotIndex >= 0
            && slotIndex < HeroDefinition.MaxTranscendSlots
            && Stars >= HeroDefinition.GetTranscendRequiredStars(slotIndex);
    }

    public string GetTranscendOptionId(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= transcendOptions.Count)
        {
            return string.Empty;
        }

        return transcendOptions[slotIndex].OptionId;
    }

    public void SetTranscendOptionId(int slotIndex, string optionId)
    {
        if (slotIndex < 0 || slotIndex >= transcendOptions.Count)
        {
            return;
        }

        transcendOptions[slotIndex].OptionId = optionId ?? string.Empty;
    }
}
