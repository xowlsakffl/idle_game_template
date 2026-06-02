using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using IdleGame.App;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Gacha;
using IdleGame.Progression;
using IdleGame.Save;
using IdleGame.Speed;

namespace IdleGame.Editor
{
    public static partial class IdleGameQaRunner
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
    }
}
