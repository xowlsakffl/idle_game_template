using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class IdleGameQaRunner
{
    [MenuItem("Tools/Idle Game/Run QA")]
    public static void RunAll()
    {
        using (var prefs = new PlayerPrefsScope())
        {
            RunWithFreshPrefs(prefs, TestAutoProgress);
            RunWithFreshPrefs(prefs, TestSelectedStageRepeats);
            RunWithFreshPrefs(prefs, TestBossFailureAndGrowthRetry);
            RunWithFreshPrefs(prefs, TestCombatSpeedEntitlement);
            RunWithFreshPrefs(prefs, TestGachaAndSaveRestore);
            RunWithFreshPrefs(prefs, TestHeroStarUp);
            TestOfflineRewardFormula();
        }

        Debug.Log("IdleGame QA passed.");
    }

    [MenuItem("Tools/Idle Game/Run Balance Report")]
    public static void RunBalanceReport()
    {
        using (var prefs = new PlayerPrefsScope())
        {
            prefs.ClearKnownKeys();
            using (var game = new RuntimeHarness())
            {
                SpendAffordableGrowth(game);
                float firstRunSeconds = SimulateUntilState(
                    game,
                    () => game.Progress.Mode == ProgressMode.BossBlocked || game.Progress.ChapterOneBossCleared,
                    3600f,
                    true);

                Debug.Log(
                    "Balance first run: time=" + FormatSeconds(firstRunSeconds)
                    + ", stage=" + game.Progress.CurrentStageId
                    + ", mode=" + game.Progress.Mode
                    + ", gold=" + game.Wallet.Gold
                    + ", exp=" + game.Wallet.HeroExpItem
                    + ", heroes=" + FormatHeroLevels(game.Battle)
                    + ", abilities=" + FormatAbilityLevels(game.Abilities));

                for (int cycle = 1; cycle <= 5 && !game.Progress.ChapterOneBossCleared; cycle++)
                {
                    game.Battle.DebugSimulateSeconds(300f);
                    SpendAffordableGrowth(game);
                    game.Progress.ResumeAutoProgress();

                    float retrySeconds = SimulateUntilState(
                        game,
                        () => game.Progress.Mode != ProgressMode.AutoProgress || game.Progress.ChapterOneBossCleared,
                        90f,
                        true);

                    Debug.Log(
                        "Balance retry " + cycle
                        + ": bossWindow=" + FormatSeconds(retrySeconds)
                        + ", stage=" + game.Progress.CurrentStageId
                        + ", mode=" + game.Progress.Mode
                        + ", cleared=" + game.Progress.ChapterOneBossCleared
                        + ", gold=" + game.Wallet.Gold
                        + ", exp=" + game.Wallet.HeroExpItem
                        + ", heroes=" + FormatHeroLevels(game.Battle)
                        + ", abilities=" + FormatAbilityLevels(game.Abilities));
                }
            }
        }
    }

    private static void TestAutoProgress(RuntimeHarness game)
    {
        AssertEqual("1-1", game.Progress.CurrentStageId, "new game starts at 1-1");
        SimulateUntil(game, () => game.Progress.CurrentStageId == "1-2", 240f, "1-1 should auto-progress to 1-2");
        AssertEqual(ProgressMode.AutoProgress, game.Progress.Mode, "auto progress mode remains active");
        AssertTrue(game.Wallet.Gold > 0, "normal kills should grant gold");
        AssertTrue(game.Wallet.HeroExpItem > 120, "normal kills should grant hero EXP items");
    }

    private static void TestSelectedStageRepeats(RuntimeHarness game)
    {
        game.Progress.DebugUnlockThrough("1-5");
        AssertTrue(game.Progress.SelectStage("1-1"), "unlocked stage can be selected");
        AssertEqual(ProgressMode.RepeatSelected, game.Progress.Mode, "stage selection enters repeat mode");

        game.Battle.DebugSimulateSeconds(240f);

        AssertEqual("1-1", game.Progress.CurrentStageId, "selected stage should keep repeating");
        AssertEqual(ProgressMode.RepeatSelected, game.Progress.Mode, "repeat mode should not auto-progress");
    }

    private static void TestBossFailureAndGrowthRetry(RuntimeHarness game)
    {
        game.Progress.DebugJumpToStage(GameData.ChapterOneBossStageId, ProgressMode.AutoProgress);
        game.Battle.DebugSimulateSeconds(31f);

        AssertEqual(GameData.BossFallbackStageId, game.Progress.CurrentStageId, "failed boss should fall back to 1-19");
        AssertEqual(ProgressMode.BossBlocked, game.Progress.Mode, "boss failure should enter blocked mode");
        AssertTrue(!game.Progress.ChapterOneBossCleared, "failed boss should not clear chapter");

        game.Battle.DebugLevelAllHeroes(40);
        game.Progress.ResumeAutoProgress();
        AssertEqual(GameData.ChapterOneBossStageId, game.Progress.CurrentStageId, "resume should retry highest boss");

        SimulateUntil(game, () => game.Progress.ChapterOneBossCleared, 20f, "leveled heroes should clear boss");

        AssertEqual(GameData.BossFallbackStageId, game.Progress.CurrentStageId, "cleared chapter should farm 1-19");
        AssertEqual(ProgressMode.RepeatSelected, game.Progress.Mode, "cleared chapter should settle into repeat farming");
    }

    private static void TestCombatSpeedEntitlement(RuntimeHarness game)
    {
        AssertTrue(game.Speed.TrySelectSpeed(GameSpeedManager.FreeSpeed), "2x speed should be free");
        AssertEqual(GameSpeedManager.FreeSpeed, game.Speed.CurrentMultiplier, "2x speed should be selected");

        AssertTrue(!game.Speed.CanUseSpeed(GameSpeedManager.PremiumSpeed), "4x speed should start locked");
        AssertTrue(!game.Speed.TrySelectSpeed(GameSpeedManager.PremiumSpeed), "locked 4x speed should not be selected");
        AssertEqual(GameSpeedManager.FreeSpeed, game.Speed.CurrentMultiplier, "locked 4x attempt should keep current speed");

        game.Speed.DebugSetFourTimesEntitlement(true);
        AssertTrue(game.Speed.TrySelectSpeed(GameSpeedManager.PremiumSpeed), "4x speed should work after entitlement");
        AssertEqual(GameSpeedManager.PremiumSpeed, game.Speed.CurrentMultiplier, "4x speed should be selected after entitlement");
    }

    private static void TestGachaAndSaveRestore(RuntimeHarness game)
    {
        game.Gacha.Roll(10);
        AssertEqual(0L, game.Wallet.HeroSummonTicket, "ten-roll should spend default tickets");
        AssertTrue(GetTotalShards(game.Battle) > 0, "ten-roll should add hero shards");

        game.Wallet.AddGold(1234);
        game.Progress.DebugJumpToStage("1-5", ProgressMode.RepeatSelected);
        long savedGold = game.Wallet.Gold;

        game.Dispose();

        using (var restored = new RuntimeHarness())
        {
            AssertEqual("1-5", restored.Progress.CurrentStageId, "stage should restore from PlayerPrefs");
            AssertEqual(ProgressMode.RepeatSelected, restored.Progress.Mode, "mode should restore from PlayerPrefs");
            AssertEqual(savedGold, restored.Wallet.Gold, "gold should restore from PlayerPrefs");
            AssertTrue(GetTotalShards(restored.Battle) > 0, "hero shards should restore from PlayerPrefs");
        }
    }

    private static void TestHeroStarUp(RuntimeHarness game)
    {
        HeroState hero = FindHeroState(game.Battle, "H001");
        int attackBefore = hero.AttackPower;
        game.Battle.AddHeroShards(hero.Definition.Id, hero.StarUpCost);

        AssertTrue(game.Battle.TryStarUpHero(hero.Definition.Id), "hero should star up when enough shards exist");
        AssertEqual(1, hero.Stars, "hero star level should increase");
        AssertEqual(0, hero.Shards, "star up should spend required shards");
        AssertTrue(hero.AttackPower > attackBefore, "star up should increase attack power");

        game.Dispose();

        using (var restored = new RuntimeHarness())
        {
            HeroState restoredHero = FindHeroState(restored.Battle, "H001");
            AssertEqual(1, restoredHero.Stars, "hero star level should restore from PlayerPrefs");
        }
    }

    private static void TestOfflineRewardFormula()
    {
        DateTime now = new DateTime(2026, 5, 14, 0, 0, 0, DateTimeKind.Utc);
        long oneHourReward = GameBootstrap.CalculateOfflineGoldReward(now.AddHours(-1), now, "1-19");
        long expectedOneHour = (long)Math.Floor(3600 * GameData.GetOfflineGoldPerSecond("1-19"));
        AssertEqual(expectedOneHour, oneHourReward, "offline reward should use selected farming stage rate");

        long cappedReward = GameBootstrap.CalculateOfflineGoldReward(now.AddHours(-10), now, "1-19");
        long expectedCap = (long)Math.Floor(28800 * GameData.GetOfflineGoldPerSecond("1-19"));
        AssertEqual(expectedCap, cappedReward, "offline reward should cap at 8 hours");

        long shortReward = GameBootstrap.CalculateOfflineGoldReward(now.AddSeconds(-10), now, "1-19");
        AssertEqual(0L, shortReward, "short sessions should not receive offline reward");
    }

    private static void RunWithFreshPrefs(PlayerPrefsScope prefs, Action<RuntimeHarness> test)
    {
        prefs.ClearKnownKeys();
        using (var game = new RuntimeHarness())
        {
            test(game);
        }
    }

    private static void SimulateUntil(RuntimeHarness game, Func<bool> condition, float timeoutSeconds, string failureMessage)
    {
        float elapsed = 0f;
        while (!condition() && elapsed < timeoutSeconds)
        {
            game.Battle.DebugSimulateSeconds(0.1f);
            elapsed += 0.1f;
        }

        AssertTrue(condition(), failureMessage + " within " + timeoutSeconds + " seconds");
    }

    private static float SimulateUntilState(
        RuntimeHarness game,
        Func<bool> condition,
        float timeoutSeconds,
        bool spendExp)
    {
        float elapsed = 0f;
        while (!condition() && elapsed < timeoutSeconds)
        {
            game.Battle.DebugSimulateSeconds(0.25f);
            if (spendExp)
            {
                SpendAffordableGrowth(game);
            }

            elapsed += 0.25f;
        }

        return elapsed;
    }

    private static int SpendCheapestHeroLevels(RuntimeHarness game)
    {
        int upgrades = 0;
        bool upgraded;
        do
        {
            upgraded = false;
            HeroState cheapest = null;
            foreach (HeroState hero in game.Battle.Heroes)
            {
                if (hero.LevelUpCost <= game.Wallet.HeroExpItem
                    && (cheapest == null || hero.LevelUpCost < cheapest.LevelUpCost))
                {
                    cheapest = hero;
                }
            }

            if (cheapest != null && game.Battle.TryLevelUpHero(cheapest.Definition.Id))
            {
                upgrades += 1;
                upgraded = true;
            }
        }
        while (upgraded);

        return upgrades;
    }

    private static int SpendCheapestAccountAbilities(RuntimeHarness game)
    {
        int upgrades = 0;
        bool upgraded;
        do
        {
            upgraded = false;
            AbilityState cheapest = null;
            foreach (AbilityState ability in game.Abilities.States)
            {
                if (!ability.IsMaxed
                    && ability.LevelUpCost <= game.Wallet.Gold
                    && (cheapest == null || ability.LevelUpCost < cheapest.LevelUpCost))
                {
                    cheapest = ability;
                }
            }

            if (cheapest != null && game.Abilities.TryLevelUp(cheapest.Definition.Kind))
            {
                upgrades += 1;
                upgraded = true;
            }
        }
        while (upgraded);

        return upgrades;
    }

    private static int SpendAffordableGrowth(RuntimeHarness game)
    {
        int upgrades;
        int total = 0;
        do
        {
            upgrades = SpendCheapestHeroLevels(game) + SpendCheapestAccountAbilities(game);
            total += upgrades;
        }
        while (upgrades > 0);

        return total;
    }

    private static string FormatHeroLevels(BattleManager battle)
    {
        var parts = new List<string>();
        foreach (HeroState hero in battle.Heroes)
        {
            parts.Add(hero.Definition.Id + ":Lv" + hero.Level + "/S" + hero.Stars + "/ATK" + hero.AttackPower);
        }

        return string.Join(", ", parts);
    }

    private static string FormatAbilityLevels(AbilityManager abilities)
    {
        var parts = new List<string>();
        foreach (AbilityState ability in abilities.States)
        {
            parts.Add(ability.Definition.Id + ":Lv" + ability.Level);
        }

        return string.Join(", ", parts);
    }

    private static string FormatSeconds(float seconds)
    {
        return seconds.ToString("0.0") + "s";
    }

    private static int GetTotalShards(BattleManager battle)
    {
        int total = 0;
        foreach (HeroState hero in battle.Heroes)
        {
            total += hero.Shards;
        }

        return total;
    }

    private static HeroState FindHeroState(BattleManager battle, string heroId)
    {
        foreach (HeroState hero in battle.Heroes)
        {
            if (hero.Definition.Id == heroId)
            {
                return hero;
            }
        }

        throw new InvalidOperationException("QA failed: missing hero " + heroId);
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException("QA failed: " + message);
        }
    }

    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException(
                "QA failed: " + message + " Expected=" + expected + " Actual=" + actual);
        }
    }

    private sealed class RuntimeHarness : IDisposable
    {
        private GameObject root;

        public RuntimeHarness()
        {
            root = new GameObject("IdleGameQaRuntime");
            Save = root.AddComponent<SaveManager>();
            Progress = root.AddComponent<StageProgressManager>();
            Wallet = root.AddComponent<CurrencyWallet>();
            Abilities = root.AddComponent<AbilityManager>();
            Speed = root.AddComponent<GameSpeedManager>();
            Battle = root.AddComponent<BattleManager>();
            Gacha = root.AddComponent<GachaManager>();

            Progress.Initialize(Save);
            Wallet.Initialize(Save);
            Abilities.Initialize(Wallet, Save);
            Speed.Initialize(Save);
            Battle.Initialize(Progress, Wallet, Save, Abilities, Speed);
            Gacha.Initialize(Battle, Wallet);
        }

        public SaveManager Save { get; }
        public StageProgressManager Progress { get; }
        public CurrencyWallet Wallet { get; }
        public AbilityManager Abilities { get; }
        public GameSpeedManager Speed { get; }
        public BattleManager Battle { get; }
        public GachaManager Gacha { get; }

        public void Dispose()
        {
            if (root != null)
            {
                UnityEngine.Object.DestroyImmediate(root);
                root = null;
            }
        }
    }

    private sealed class PlayerPrefsScope : IDisposable
    {
        private readonly List<PrefEntry> snapshot = new List<PrefEntry>();

        public PlayerPrefsScope()
        {
            foreach (PrefDescriptor pref in GetKnownPrefs())
            {
                snapshot.Add(PrefEntry.Capture(pref));
            }
        }

        public void ClearKnownKeys()
        {
            foreach (PrefDescriptor pref in GetKnownPrefs())
            {
                PlayerPrefs.DeleteKey(pref.Key);
            }

            PlayerPrefs.Save();
        }

        public void Dispose()
        {
            ClearKnownKeys();
            foreach (PrefEntry entry in snapshot)
            {
                entry.Restore();
            }

            PlayerPrefs.Save();
        }

        private static IEnumerable<PrefDescriptor> GetKnownPrefs()
        {
            yield return PrefDescriptor.String(SaveKeys.Gold);
            yield return PrefDescriptor.String(SaveKeys.Ruby);
            yield return PrefDescriptor.String(SaveKeys.HeroExpItem);
            yield return PrefDescriptor.String(SaveKeys.HeroSummonTicket);
            yield return PrefDescriptor.String(SaveKeys.HighestStageId);
            yield return PrefDescriptor.String(SaveKeys.CurrentStageId);
            yield return PrefDescriptor.String(SaveKeys.SelectedStageId);
            yield return PrefDescriptor.String(SaveKeys.ProgressMode);
            yield return PrefDescriptor.Int(SaveKeys.ChapterOneBossCleared);
            yield return PrefDescriptor.String(SaveKeys.LastOnlineUtcTicks);
            yield return PrefDescriptor.String(SaveKeys.CombatSpeedMultiplier);
            yield return PrefDescriptor.Int(SaveKeys.HasFourTimesSpeedEntitlement);

            foreach (HeroDefinition hero in GameData.Heroes)
            {
                yield return PrefDescriptor.Int(SaveKeys.HeroLevel(hero.Id));
                yield return PrefDescriptor.Int(SaveKeys.HeroShards(hero.Id));
                yield return PrefDescriptor.Int(SaveKeys.HeroStars(hero.Id));
            }

            foreach (AbilityDefinition ability in GameData.Abilities)
            {
                yield return PrefDescriptor.Int(SaveKeys.AbilityLevel(ability.Kind));
            }
        }
    }

    private enum PrefKind
    {
        String,
        Int
    }

    private readonly struct PrefDescriptor
    {
        private PrefDescriptor(string key, PrefKind kind)
        {
            Key = key;
            Kind = kind;
        }

        public string Key { get; }
        public PrefKind Kind { get; }

        public static PrefDescriptor String(string key)
        {
            return new PrefDescriptor(key, PrefKind.String);
        }

        public static PrefDescriptor Int(string key)
        {
            return new PrefDescriptor(key, PrefKind.Int);
        }
    }

    private readonly struct PrefEntry
    {
        private readonly string key;
        private readonly PrefKind kind;
        private readonly bool exists;
        private readonly string stringValue;
        private readonly int intValue;

        private PrefEntry(PrefDescriptor pref, bool exists, string stringValue, int intValue)
        {
            key = pref.Key;
            kind = pref.Kind;
            this.exists = exists;
            this.stringValue = stringValue;
            this.intValue = intValue;
        }

        public static PrefEntry Capture(PrefDescriptor pref)
        {
            bool exists = PlayerPrefs.HasKey(pref.Key);
            string stringValue = pref.Kind == PrefKind.String ? PlayerPrefs.GetString(pref.Key, string.Empty) : string.Empty;
            int intValue = pref.Kind == PrefKind.Int ? PlayerPrefs.GetInt(pref.Key, 0) : 0;
            return new PrefEntry(pref, exists, stringValue, intValue);
        }

        public void Restore()
        {
            if (exists)
            {
                if (kind == PrefKind.String)
                {
                    PlayerPrefs.SetString(key, stringValue);
                }
                else
                {
                    PlayerPrefs.SetInt(key, intValue);
                }
            }
        }
    }
}
