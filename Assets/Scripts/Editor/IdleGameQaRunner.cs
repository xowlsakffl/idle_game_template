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
            RunWithFreshPrefs(prefs, TestBattleAutoControls);
            RunWithFreshPrefs(prefs, TestHeroDamageMeter);
            RunWithFreshPrefs(prefs, TestGachaAndSaveRestore);
            RunWithFreshPrefs(prefs, TestHeroStarUp);
            RunWithFreshPrefs(prefs, TestHeroBulkStarUp);
            TestGachaRateTable();
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
        AssertEqual(GameSpeedManager.NormalSpeed, game.Speed.CurrentMultiplier, "combat speed should start at 1x");
        game.Speed.CycleSpeed();
        AssertEqual(GameSpeedManager.FreeSpeed, game.Speed.CurrentMultiplier, "first cycle click should select 2x");
        game.Speed.CycleSpeed();
        AssertEqual(GameSpeedManager.NormalSpeed, game.Speed.CurrentMultiplier, "cycle should skip locked 4x and return to 1x");

        AssertTrue(game.Speed.TrySelectSpeed(GameSpeedManager.FreeSpeed), "2x speed should be free");
        AssertEqual(GameSpeedManager.FreeSpeed, game.Speed.CurrentMultiplier, "2x speed should be selected");

        AssertTrue(!game.Speed.CanUseSpeed(GameSpeedManager.PremiumSpeed), "4x speed should start locked");
        AssertTrue(!game.Speed.TrySelectSpeed(GameSpeedManager.PremiumSpeed), "locked 4x speed should not be selected");
        AssertEqual(GameSpeedManager.FreeSpeed, game.Speed.CurrentMultiplier, "locked 4x attempt should keep current speed");

        game.Speed.DebugSetFourTimesEntitlement(true);
        game.Speed.CycleSpeed();
        AssertEqual(GameSpeedManager.PremiumSpeed, game.Speed.CurrentMultiplier, "cycle should select 4x after entitlement");
        game.Speed.CycleSpeed();
        AssertEqual(GameSpeedManager.NormalSpeed, game.Speed.CurrentMultiplier, "cycle should return from 4x to 1x");
        AssertTrue(game.Speed.TrySelectSpeed(GameSpeedManager.PremiumSpeed), "4x speed should work after entitlement");
    }

    private static void TestBattleAutoControls(RuntimeHarness game)
    {
        AssertTrue(game.Battle.SkillAutoEnabled, "skill auto should start enabled");
        AssertTrue(game.Battle.FeverAutoEnabled, "fever auto should start enabled");

        game.Battle.ToggleSkillAuto();
        game.Battle.ToggleFeverAuto();
        AssertTrue(!game.Battle.SkillAutoEnabled, "skill auto toggle should turn off");
        AssertTrue(!game.Battle.FeverAutoEnabled, "fever auto toggle should turn off");

        game.Battle.ToggleSkillAuto();
        game.Battle.ToggleFeverAuto();
        AssertTrue(game.Battle.SkillAutoEnabled, "skill auto toggle should turn on");
        AssertTrue(game.Battle.FeverAutoEnabled, "fever auto toggle should turn on");
    }

    private static void TestHeroDamageMeter(RuntimeHarness game)
    {
        HeroState firstDeployedHero = game.Battle.DeployedHeroes[0];
        IReadOnlyList<HeroState> deployedHeroes = game.Battle.DeployedHeroes;
        game.Progress.DebugJumpToStage(GameData.ChapterOneBossStageId, ProgressMode.AutoProgress);
        game.Battle.DebugSimulateSeconds(0.4f, 0.05f);

        AssertEqual(
            deployedHeroes.Count,
            game.Battle.RecentHeroAttackIds.Count,
            "all deployed heroes should attack in the first ready batch");

        foreach (HeroState hero in deployedHeroes)
        {
            AssertTrue(
                game.Battle.GetHeroDamageDone(hero.Definition.Id) > 0d,
                "each deployed hero should contribute damage when the party attacks together");
        }

        game.Battle.DebugSimulateSeconds(3f);

        AssertTrue(game.Battle.GetMaxHeroDamageDone() > GameNumber.Zero, "damage meter should track deployed hero damage");
        AssertTrue(
            game.Battle.GetHeroDamageDone(firstDeployedHero.Definition.Id) > 0d,
            "damage meter should expose damage by deployed hero id");

        game.Progress.DebugJumpToStage(GameData.BossFallbackStageId, ProgressMode.RepeatSelected);
        AssertTrue(game.Battle.VisibleEnemyCount > 1, "normal stages should spawn multiple visible enemies");
        game.Battle.DebugSimulateSeconds(0.7f, 0.05f);

        int damagedEnemyCount = 0;
        for (int i = 0; i < game.Battle.VisibleEnemyCount; i++)
        {
            if (game.Battle.GetVisibleEnemyHpRatio(i) < 1f)
            {
                damagedEnemyCount += 1;
            }
        }

        AssertTrue(damagedEnemyCount > 1, "party attacks should damage multiple visible enemies instead of one target only");

        int firstHeroTarget = game.Battle.GetHeroTargetSpawnSequence(firstDeployedHero.Definition.Id);
        AssertTrue(firstHeroTarget >= 0, "hero should lock a visible enemy target after attacking");
        game.Battle.DebugSimulateSeconds(0.1f, 0.05f);
        AssertEqual(
            firstHeroTarget,
            game.Battle.GetHeroTargetSpawnSequence(firstDeployedHero.Definition.Id),
            "hero should keep attacking the same target until that target dies");

        game.Progress.DebugJumpToStage("1-1", ProgressMode.RepeatSelected);
        int visibleBeforeRefill = game.Battle.VisibleEnemyCount;
        int maxSpawnBeforeRefill = GetMaxVisibleEnemySpawnSequence(game.Battle);
        SimulateUntil(game, () => game.Battle.KillsThisStage > 0, 5f, "normal stages should kill at least one visible enemy");
        AssertEqual(visibleBeforeRefill, game.Battle.VisibleEnemyCount, "normal stages should refill the killed visible enemy slot");
        AssertTrue(
            GetMaxVisibleEnemySpawnSequence(game.Battle) > maxSpawnBeforeRefill,
            "normal stages should spawn a new enemy after a visible enemy dies");

        game.Progress.DebugJumpToStage("1-2", ProgressMode.RepeatSelected);
        AssertEqual(GameNumber.Zero, game.Battle.GetMaxHeroDamageDone(), "damage meter should reset when a new stage starts");
    }

    private static void TestGachaAndSaveRestore(RuntimeHarness game)
    {
        game.Gacha.Roll(3);
        AssertEqual(0L, game.Wallet.HeroSummonTicket, "starter hero pulls should spend default tickets");
        AssertEqual(3, GetTotalShards(game.Battle), "starter hero pulls should add one hero shard per pull");
        game.Gacha.RollEquipment(3);
        AssertEqual(0L, game.Wallet.EquipmentSummonTicket, "starter equipment pulls should spend default tickets");
        AssertEqual(3, game.EquipmentInventory.GetTotalOwnedCount(), "starter equipment pulls should add one equipment copy per pull");

        game.Wallet.AddGold(1234);
        game.Progress.DebugJumpToStage("1-5", ProgressMode.RepeatSelected);
        EquipmentState equipmentToEquip = FindFirstOwnedEquipment(game.EquipmentInventory);
        string savedEquipmentId = equipmentToEquip.Definition.Id;
        EquipmentSlot savedEquipmentSlot = equipmentToEquip.Definition.Slot;
        AssertTrue(game.EquipmentInventory.Equip("H001", savedEquipmentId), "owned equipment should equip to hero");
        AssertEqual(savedEquipmentId, game.EquipmentInventory.GetEquippedEquipmentId("H001", savedEquipmentSlot), "equipped equipment should be stored on hero slot");

        GameNumber savedGold = game.Wallet.Gold;
        int savedEquipmentCount = game.EquipmentInventory.GetTotalOwnedCount();

        game.Dispose();

        using (var restored = new RuntimeHarness())
        {
            AssertEqual("1-5", restored.Progress.CurrentStageId, "stage should restore from PlayerPrefs");
            AssertEqual(ProgressMode.RepeatSelected, restored.Progress.Mode, "mode should restore from PlayerPrefs");
            AssertEqual(savedGold, restored.Wallet.Gold, "gold should restore from PlayerPrefs");
            AssertTrue(GetTotalShards(restored.Battle) > 0, "hero shards should restore from PlayerPrefs");
            AssertEqual(savedEquipmentCount, restored.EquipmentInventory.GetTotalOwnedCount(), "equipment inventory should restore from PlayerPrefs");
            AssertEqual(savedEquipmentId, restored.EquipmentInventory.GetEquippedEquipmentId("H001", savedEquipmentSlot), "equipped equipment should restore from PlayerPrefs");
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

    private static void TestHeroBulkStarUp(RuntimeHarness game)
    {
        HeroState firstHero = FindHeroState(game.Battle, "H001");
        HeroState secondHero = FindHeroState(game.Battle, "H002");
        int firstHeroTwoStarCost = firstHero.Definition.GetStarUpCost(0) + firstHero.Definition.GetStarUpCost(1);
        game.Battle.AddHeroShards(firstHero.Definition.Id, firstHeroTwoStarCost);
        game.Battle.AddHeroShards(secondHero.Definition.Id, secondHero.StarUpCost);

        int starUps = game.Battle.BulkStarUpHeroes();

        AssertEqual(3, starUps, "bulk star up should process every affordable star level");
        AssertEqual(2, firstHero.Stars, "bulk star up should repeat for a hero while shards are enough");
        AssertEqual(1, secondHero.Stars, "bulk star up should process multiple heroes");
        AssertEqual(0, firstHero.Shards, "bulk star up should spend repeated costs");
        AssertEqual(0, secondHero.Shards, "bulk star up should spend single costs");
    }

    private static void TestGachaRateTable()
    {
        AssertEqual(10000, GachaManager.GetTotalRateWeight(), "gacha rarity weights should sum to 100%");
        AssertEqual(4500, GachaManager.GetRarityRateWeight(HeroRarity.Common), "common rate should be 45%");
        AssertEqual(3000, GachaManager.GetRarityRateWeight(HeroRarity.Uncommon), "uncommon rate should be 30%");
        AssertEqual(1500, GachaManager.GetRarityRateWeight(HeroRarity.Rare), "rare rate should be 15%");
        AssertEqual(700, GachaManager.GetRarityRateWeight(HeroRarity.Epic), "epic rate should be 7%");
        AssertEqual(250, GachaManager.GetRarityRateWeight(HeroRarity.Legendary), "legendary rate should be 2.5%");
        AssertEqual(50, GachaManager.GetRarityRateWeight(HeroRarity.Mythic), "mythic rate should be 0.5%");
    }

    private static void TestOfflineRewardFormula()
    {
        DateTime now = new DateTime(2026, 5, 14, 0, 0, 0, DateTimeKind.Utc);
        GameNumber oneHourReward = GameBootstrap.CalculateOfflineGoldReward(now.AddHours(-1), now, "1-19");
        GameNumber expectedOneHour = GameNumber.Floor(GameData.GetOfflineGoldPerSecond("1-19") * 3600d);
        AssertEqual(expectedOneHour, oneHourReward, "offline reward should use selected farming stage rate");

        GameNumber cappedReward = GameBootstrap.CalculateOfflineGoldReward(now.AddHours(-10), now, "1-19");
        GameNumber expectedCap = GameNumber.Floor(GameData.GetOfflineGoldPerSecond("1-19") * 28800d);
        AssertEqual(expectedCap, cappedReward, "offline reward should cap at 8 hours");

        GameNumber shortReward = GameBootstrap.CalculateOfflineGoldReward(now.AddSeconds(-10), now, "1-19");
        AssertEqual(GameNumber.Zero, shortReward, "short sessions should not receive offline reward");
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

    private static int GetMaxVisibleEnemySpawnSequence(BattleManager battle)
    {
        int maxSequence = -1;
        for (int i = 0; i < battle.VisibleEnemyCount; i++)
        {
            maxSequence = Math.Max(maxSequence, battle.GetVisibleEnemySpawnSequence(i));
        }

        return maxSequence;
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

    private static EquipmentState FindFirstOwnedEquipment(EquipmentInventory equipmentInventory)
    {
        foreach (EquipmentState state in equipmentInventory.States)
        {
            if (state.IsOwned)
            {
                return state;
            }
        }

        throw new InvalidOperationException("QA failed: missing owned equipment");
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
            EquipmentInventory = root.AddComponent<EquipmentInventory>();
            Speed = root.AddComponent<GameSpeedManager>();
            Battle = root.AddComponent<BattleManager>();
            Gacha = root.AddComponent<GachaManager>();

            Progress.Initialize(Save);
            Wallet.Initialize(Save);
            Abilities.Initialize(Wallet, Save);
            EquipmentInventory.Initialize(Save);
            Speed.Initialize(Save);
            Battle.Initialize(Progress, Wallet, Save, Abilities, Speed);
            Gacha.Initialize(Battle, Wallet, EquipmentInventory);
        }

        public SaveManager Save { get; }
        public StageProgressManager Progress { get; }
        public CurrencyWallet Wallet { get; }
        public AbilityManager Abilities { get; }
        public EquipmentInventory EquipmentInventory { get; }
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
            yield return PrefDescriptor.String(SaveKeys.EquipmentSummonTicket);
            yield return PrefDescriptor.String(SaveKeys.HighestStageId);
            yield return PrefDescriptor.String(SaveKeys.CurrentStageId);
            yield return PrefDescriptor.String(SaveKeys.SelectedStageId);
            yield return PrefDescriptor.String(SaveKeys.ProgressMode);
            yield return PrefDescriptor.Int(SaveKeys.ChapterOneBossCleared);
            yield return PrefDescriptor.String(SaveKeys.LastOnlineUtcTicks);
            yield return PrefDescriptor.String(SaveKeys.CombatSpeedMultiplier);
            yield return PrefDescriptor.Int(SaveKeys.HasFourTimesSpeedEntitlement);
            yield return PrefDescriptor.Int(SaveKeys.SkillAutoEnabled);
            yield return PrefDescriptor.Int(SaveKeys.FeverAutoEnabled);

            foreach (HeroDefinition hero in GameData.Heroes)
            {
                yield return PrefDescriptor.Int(SaveKeys.HeroLevel(hero.Id));
                yield return PrefDescriptor.Int(SaveKeys.HeroShards(hero.Id));
                yield return PrefDescriptor.Int(SaveKeys.HeroStars(hero.Id));
                foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
                {
                    yield return PrefDescriptor.String(SaveKeys.HeroEquipmentSlot(hero.Id, slot));
                }
            }

            foreach (EquipmentDefinition equipment in GameData.Equipments)
            {
                yield return PrefDescriptor.Int(SaveKeys.EquipmentLevel(equipment.Id));
                yield return PrefDescriptor.Int(SaveKeys.EquipmentStars(equipment.Id));
                yield return PrefDescriptor.Int(SaveKeys.EquipmentCount(equipment.Id));
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
