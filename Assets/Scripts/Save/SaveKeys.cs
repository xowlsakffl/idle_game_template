public static class SaveKeys
{
    public const string Gold = "gold";
    public const string Ruby = "ruby";
    public const string HeroExpItem = "heroExpItem";
    public const string EquipmentExpItem = "equipmentExpItem";
    public const string HeroSummonTicket = "heroSummonTicket";
    public const string EquipmentSummonTicket = "equipmentSummonTicket";
    public const string HighestStageId = "highestStageId";
    public const string CurrentStageId = "currentStageId";
    public const string SelectedStageId = "selectedStageId";
    public const string ProgressMode = "progressMode";
    public const string ChapterOneBossCleared = "chapter1BossCleared";
    public const string LastOnlineUtcTicks = "lastOnlineUtcTicks";
    public const string CombatSpeedMultiplier = "combatSpeedMultiplier";
    public const string HasFourTimesSpeedEntitlement = "hasFourTimesSpeedEntitlement";
    public const string SkillAutoEnabled = "skillAutoEnabled";
    public const string FeverAutoEnabled = "feverAutoEnabled";
    public const string HeroFormationPreset = "heroFormationPreset";

    public static string HeroLevel(string heroId)
    {
        return "hero." + heroId + ".level";
    }

    public static string HeroShards(string heroId)
    {
        return "hero." + heroId + ".shards";
    }

    public static string HeroStars(string heroId)
    {
        return "hero." + heroId + ".stars";
    }

    public static string HeroFormationSlot(int preset, int slot)
    {
        return "heroFormation." + preset + ".slot." + slot;
    }

    public static string EquipmentCount(string equipmentId)
    {
        return "equipment." + equipmentId + ".count";
    }

    public static string EquipmentLevel(string equipmentId)
    {
        return "equipment." + equipmentId + ".level";
    }

    public static string EquipmentStars(string equipmentId)
    {
        return "equipment." + equipmentId + ".stars";
    }

    public static string HeroEquipmentSlot(string heroId, EquipmentSlot slot)
    {
        return "hero." + heroId + ".equipment." + slot;
    }

    public static string AbilityLevel(AbilityKind kind)
    {
        return "ability." + kind + ".level";
    }
}
