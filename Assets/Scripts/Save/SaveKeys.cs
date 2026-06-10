using IdleGame.Data;
using IdleGame.Progression;

namespace IdleGame.Save
{
    public static class SaveKeys
    {
        public const string Gold = "gold";
        public const string Ruby = "ruby";
        public const string HeroExpItem = "heroExpItem";
        public const string EquipmentExpItem = "equipmentExpItem";
        public const string TotemEssence = "totemEssence";
        public const string RuneDust = "runeDust"; // Legacy: rune growth now uses same-rune synthesis.
        public const string Wood = "wood";
        public const string Brick = "brick";
        public const string Iron = "iron";
        public const string HeroTranscendStone = "heroTranscendStone";
        public const string HeroSummonTicket = "heroSummonTicket";
        public const string EquipmentSummonTicket = "equipmentSummonTicket";
        public const string DungeonTicket = "dungeonTicket";
        public const string DungeonFreeEntryDate = "dungeon.freeEntryDate";
        public const string DungeonFreeEntriesUsed = "dungeon.freeEntriesUsed";
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
        public const string FortressLevel = "fortress.level";
        public const string FortressExperience = "fortress.experience";

        public static string GachaTotalPulls(string poolId)
        {
            return "gacha." + poolId + ".totalPulls";
        }

        public static string DungeonHighestClearLevel(string dungeonId)
        {
            return "dungeon." + dungeonId + ".highestClearLevel";
        }

        public static string GachaPityCount(string poolId)
        {
            return "gacha." + poolId + ".pityCount";
        }

        public static string GachaPityCount(string poolId, string targetId)
        {
            return "gacha." + poolId + "." + targetId + ".pityCount";
        }

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
            // Legacy compatibility only. Totems are global progression now, not preset equipment.
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

        public static string RuneGrade(string runeId)
        {
            return "rune." + runeId + ".grade";
        }

        public static string RuneCopies(string runeId)
        {
            return "rune." + runeId + ".copies";
        }

        public static string RuneCount(string runeId, RuneGrade grade)
        {
            return "rune." + runeId + ".count." + (int)grade;
        }

        public static string RuneUnlocked(string runeId)
        {
            return "rune." + runeId + ".unlocked";
        }

        public static string FacilityLevel(string facilityId)
        {
            return "facility." + facilityId + ".level";
        }

        public static string FacilityAssignedHero(string facilityId, int slot)
        {
            return "facility." + facilityId + ".hero." + slot;
        }

        public static string FacilityStoredAmount(string facilityId)
        {
            return "facility." + facilityId + ".stored";
        }

        public static string FacilityLastUpdateUtcTicks(string facilityId)
        {
            return "facility." + facilityId + ".lastUpdateUtcTicks";
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
}
