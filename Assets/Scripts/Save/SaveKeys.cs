public static class SaveKeys
{
    public const string Gold = "gold";
    public const string Ruby = "ruby";
    public const string HeroExpItem = "heroExpItem";
    public const string HeroSummonTicket = "heroSummonTicket";
    public const string HighestStageId = "highestStageId";
    public const string CurrentStageId = "currentStageId";
    public const string SelectedStageId = "selectedStageId";
    public const string ProgressMode = "progressMode";
    public const string ChapterOneBossCleared = "chapter1BossCleared";
    public const string LastOnlineUtcTicks = "lastOnlineUtcTicks";

    public static string HeroLevel(string heroId)
    {
        return "hero." + heroId + ".level";
    }

    public static string HeroShards(string heroId)
    {
        return "hero." + heroId + ".shards";
    }
}
