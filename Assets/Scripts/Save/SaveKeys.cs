public static class SaveKeys
{
    public const string Gold = "gold";
    public const string Ruby = "ruby";
    public const string HeroExpItem = "heroExpItem";
    public const string EquipmentExpItem = "equipmentExpItem";
    public const string TotemEssence = "totemEssence";
    public const string RuneDust = "runeDust";
    public const string HeroTranscendStone = "heroTranscendStone";
    public const string HeroSummonTicket = "heroSummonTicket";
    public const string EquipmentSummonTicket = "equipmentSummonTicket";
    public const string AccountLevel = "accountLevel";
    public const string AccountExperience = "accountExperience";
    public const string DebugTalentPointBonus = "debugTalentPointBonus";
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
    public const string HeroTranscendStopOnlySs = "heroTranscendStopOnlySs";
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

    public static string HeroTranscendOption(string heroId, int slot)
    {
        return "hero." + heroId + ".transcend." + slot;
    }

    public static string HeroTranscendOptionRolled(string heroId, int slot)
    {
        return "hero." + heroId + ".transcend.rolled." + slot;
    }

    public static string HeroTranscendLocked(string heroId, int slot)
    {
        return "hero." + heroId + ".transcend.locked." + slot;
    }

    public static string HeroFormationSlot(int preset, int slot)
    {
        return "heroFormation." + preset + ".slot." + slot;
    }

    public static string HeroFormationTotem(int preset)
    {
        return "heroFormation." + preset + ".totem";
    }

    public static string HeroFormationTotem(int preset, int slot)
    {
        return slot <= 1
            ? HeroFormationTotem(preset)
            : "heroFormation." + preset + ".totem." + slot;
    }

    public static string HeroFormationRune(int preset, int slot)
    {
        return "heroFormation." + preset + ".rune." + slot;
    }

    public static string TotemLevel(string totemId)
    {
        return "totem." + totemId + ".level";
    }

    public static string TotemGrade(string totemId)
    {
        return "totem." + totemId + ".grade";
    }

    public static string TotemUnlocked(string totemId)
    {
        return "totem." + totemId + ".unlocked";
    }

    public static string RuneLevel(string runeId)
    {
        return "rune." + runeId + ".level";
    }

    public static string RuneUnlocked(string runeId)
    {
        return "rune." + runeId + ".unlocked";
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

    public static string TalentLevel(string talentId)
    {
        return "talent." + talentId + ".level";
    }
}
