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
public sealed class HeroDefinition
{
    public const int MaxStars = 15;

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
        float passiveValuePercent)
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
        PassiveValuePercent = Mathf.Max(0f, passiveValuePercent);
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
    public string RarityLabel => GetRarityLabel(Rarity);
    public string PassiveLabel => GetPassiveLabel(PassiveStat) + " +" + PassiveValuePercent.ToString("0.#") + "%";

    public int GetAttackPower(int level)
    {
        return GetAttackPower(level, 0);
    }

    public int GetAttackPower(int level, int stars)
    {
        int levelAttack = BaseAttack + Mathf.Max(0, level - 1) * AttackPerLevel;
        float starMultiplier = 1f + Mathf.Clamp(stars, 0, MaxStars) * 0.2f;
        return Mathf.Max(1, Mathf.FloorToInt(levelAttack * starMultiplier * GetPassiveMultiplier(HeroPassiveStat.AttackPower)));
    }

    public int GetMaxHp(int level, int stars)
    {
        int levelHp = BaseHp + Mathf.Max(0, level - 1) * HpPerLevel;
        float starMultiplier = 1f + Mathf.Clamp(stars, 0, MaxStars) * 0.15f;
        return Mathf.Max(1, Mathf.FloorToInt(levelHp * starMultiplier * GetPassiveMultiplier(HeroPassiveStat.MaxHp)));
    }

    public float GetAttackSpeed()
    {
        return AttackSpeed * GetPassiveMultiplier(HeroPassiveStat.AttackSpeed);
    }

    public float GetMoveSpeed()
    {
        return MoveSpeed * GetPassiveMultiplier(HeroPassiveStat.MoveSpeed);
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

    private float GetPassiveMultiplier(HeroPassiveStat stat)
    {
        return PassiveStat == stat ? 1f + PassiveValuePercent / 100f : 1f;
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
    public int MaxHp => Definition.GetMaxHp(Level, Stars);
    public float AttackSpeed => Definition.GetAttackSpeed();
    public float MoveSpeed => Definition.GetMoveSpeed();
    public float AttackInterval => Mathf.Max(0.1f, 1f / AttackSpeed);
    public int LevelUpCost => Definition.GetLevelUpCost(Level);
    public bool IsMaxStars => Stars >= HeroDefinition.MaxStars;
    public int StarUpCost => Definition.GetStarUpCost(Stars);
    public bool CanStarUp => !IsMaxStars && Shards >= StarUpCost;
}
