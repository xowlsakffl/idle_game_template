using System;
using System.Collections.Generic;
using IdleGame.App;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Gacha;
using IdleGame.Progression;
using IdleGame.Speed;

namespace IdleGame.Editor
{
    public static partial class IdleGameQaRunner
    {
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
    }
}
