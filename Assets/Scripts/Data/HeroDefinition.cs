using System;
using UnityEngine;

[Serializable]
public enum HeroRarity
{
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}

[Serializable]
public sealed class HeroDefinition
{
    public const int MaxStars = 5;

    public HeroDefinition(
        string id,
        string displayName,
        string role,
        HeroRarity rarity,
        int baseAttack,
        float attackInterval,
        int attackPerLevel)
    {
        Id = id;
        DisplayName = displayName;
        Role = role;
        Rarity = rarity;
        BaseAttack = baseAttack;
        AttackInterval = attackInterval;
        AttackPerLevel = attackPerLevel;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string Role { get; }
    public HeroRarity Rarity { get; }
    public int BaseAttack { get; }
    public float AttackInterval { get; }
    public int AttackPerLevel { get; }
    public string RarityLabel => GetRarityLabel(Rarity);

    public int GetAttackPower(int level)
    {
        return GetAttackPower(level, 0);
    }

    public int GetAttackPower(int level, int stars)
    {
        int levelAttack = BaseAttack + Mathf.Max(0, level - 1) * AttackPerLevel;
        float starMultiplier = 1f + Mathf.Clamp(stars, 0, MaxStars) * 0.2f;
        return Mathf.Max(1, Mathf.FloorToInt(levelAttack * starMultiplier));
    }

    public int GetLevelUpCost(int level)
    {
        return Mathf.FloorToInt(20f * Mathf.Pow(Mathf.Max(1, level), 1.25f));
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
        return GetBaseShardStep(Rarity);
    }

    private static int GetBaseShardStep(HeroRarity rarity)
    {
        switch (rarity)
        {
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

    private static string GetRarityLabel(HeroRarity rarity)
    {
        switch (rarity)
        {
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
}

public sealed class HeroState
{
    public HeroState(HeroDefinition definition, int level, int shards, int stars)
    {
        Definition = definition;
        Level = Mathf.Max(1, level);
        Shards = Mathf.Max(0, shards);
        Stars = Mathf.Clamp(stars, 0, HeroDefinition.MaxStars);
    }

    public HeroDefinition Definition { get; }
    public int Level { get; set; }
    public int Shards { get; set; }
    public int Stars { get; set; }
    public float AttackCooldown { get; set; }

    public int AttackPower => Definition.GetAttackPower(Level, Stars);
    public int LevelUpCost => Definition.GetLevelUpCost(Level);
    public bool IsMaxStars => Stars >= HeroDefinition.MaxStars;
    public int StarUpCost => Definition.GetStarUpCost(Stars);
    public bool CanStarUp => !IsMaxStars && Shards >= StarUpCost;
}
