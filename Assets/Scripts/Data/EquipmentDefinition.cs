public enum EquipmentSlot
{
    Weapon,
    Hat,
    Armor,
    Accessory,
    Potion
}

public sealed class EquipmentDefinition
{
    public const int MaxStars = 15;
    public const int LevelPerStar = 50;
    public const int MaxLevelAtMaxStars = (MaxStars + 1) * LevelPerStar;

    public EquipmentDefinition(
        string id,
        string displayName,
        EquipmentSlot slot,
        HeroRarity rarity,
        int attackBonus,
        int hpBonus)
    {
        Id = id;
        DisplayName = displayName;
        Slot = slot;
        Rarity = rarity;
        AttackBonus = attackBonus;
        HpBonus = hpBonus;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public EquipmentSlot Slot { get; }
    public HeroRarity Rarity { get; }
    public int AttackBonus { get; }
    public int HpBonus { get; }
    public string RarityLabel => GetRarityLabel(Rarity);
    public string SlotLabel => GetSlotLabel(Slot);

    public int GetMaxLevel(int stars)
    {
        int effectiveStars = UnityEngine.Mathf.Clamp(stars, 0, MaxStars) + 1;
        return effectiveStars * LevelPerStar;
    }

    public int GetAttackBonus(int level, int stars)
    {
        return GetScaledBonus(AttackBonus, level, stars);
    }

    public int GetHpBonus(int level, int stars)
    {
        return GetScaledBonus(HpBonus, level, stars);
    }

    public int GetLevelUpCost(int level, int stars)
    {
        int rarityMultiplier = GetRarityCostMultiplier(Rarity);
        int effectiveLevel = UnityEngine.Mathf.Max(1, level);
        int cost = UnityEngine.Mathf.FloorToInt(rarityMultiplier * 12f * UnityEngine.Mathf.Pow(effectiveLevel, 1.16f));
        return UnityEngine.Mathf.Clamp(cost, 1, GameData.MaxIntBalanceValue);
    }

    public int GetStarUpCost(int currentStars)
    {
        if (currentStars >= MaxStars)
        {
            return int.MaxValue;
        }

        return UnityEngine.Mathf.Clamp(currentStars, 0, MaxStars - 1) + 1;
    }

    private static int GetScaledBonus(int baseBonus, int level, int stars)
    {
        if (baseBonus <= 0)
        {
            return 0;
        }

        float levelMultiplier = 1f + UnityEngine.Mathf.Max(0, level - 1) * 0.01f;
        float starMultiplier = 1f + UnityEngine.Mathf.Clamp(stars, 0, MaxStars) * 0.055f;
        return UnityEngine.Mathf.Clamp(UnityEngine.Mathf.FloorToInt(baseBonus * levelMultiplier * starMultiplier), 1, GameData.MaxIntBalanceValue);
    }

    private static int GetRarityCostMultiplier(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Common:
                return 1;
            case HeroRarity.Uncommon:
                return 3;
            case HeroRarity.Rare:
                return 5;
            case HeroRarity.Epic:
                return 8;
            case HeroRarity.Legendary:
                return 13;
            case HeroRarity.Mythic:
                return 20;
            default:
                return 1;
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
                return "미정";
        }
    }

    private static string GetSlotLabel(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.Weapon:
                return "무기";
            case EquipmentSlot.Hat:
                return "모자";
            case EquipmentSlot.Armor:
                return "갑옷";
            case EquipmentSlot.Accessory:
                return "장신구";
            case EquipmentSlot.Potion:
                return "포션";
            default:
                return "미정";
        }
    }
}

public sealed class EquipmentState
{
    public EquipmentState(EquipmentDefinition definition, int level, int stars, int count)
    {
        Definition = definition;
        Stars = UnityEngine.Mathf.Clamp(stars, 0, EquipmentDefinition.MaxStars);
        Level = UnityEngine.Mathf.Clamp(level, 1, Definition.GetMaxLevel(Stars));
        Count = UnityEngine.Mathf.Max(0, count);
    }

    public EquipmentDefinition Definition { get; }
    public int Level { get; set; }
    public int Stars { get; set; }
    public int Count { get; set; }
    public int MaxLevel => Definition.GetMaxLevel(Stars);
    public int AttackBonus => Definition.GetAttackBonus(Level, Stars);
    public int HpBonus => Definition.GetHpBonus(Level, Stars);
    public int LevelUpCost => Definition.GetLevelUpCost(Level, Stars);
    public int StarUpCost => Definition.GetStarUpCost(Stars);
    public bool IsMaxStars => Stars >= EquipmentDefinition.MaxStars;
    public bool IsOwned => Count > 0;

    public void AddCopies(int amount)
    {
        Count = UnityEngine.Mathf.Max(0, Count + amount);
    }
}
