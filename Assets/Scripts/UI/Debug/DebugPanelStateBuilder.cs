using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Progression;
using IdleGame.Speed;

namespace IdleGame.UI.Debugging
{
    public static class DebugPanelStateBuilder
    {
        public static List<DebugPanelButtonDescriptor> BuildMainButtons(
            CurrencyWallet wallet,
            BattleManager battleManager,
            AccountProgressManager accountProgressManager,
            GameSpeedManager speedManager,
            StageProgressManager progressManager,
            Action resetSaveAction)
        {
            return new List<DebugPanelButtonDescriptor>
            {
                new DebugPanelButtonDescriptor("Gold +5000", () => wallet?.AddGold(5000)),
                new DebugPanelButtonDescriptor("EXP +5000", () => wallet?.AddHeroExpItem(5000)),
                new DebugPanelButtonDescriptor("Equip EXP +5000", () => wallet?.AddEquipmentExpItem(5000)),
                new DebugPanelButtonDescriptor("Totem Essence +5000", () => wallet?.AddTotemEssence(5000)),
                new DebugPanelButtonDescriptor("Ruby +1500", () => wallet?.AddRuby(1500)),
                new DebugPanelButtonDescriptor("Transcend +100", () => wallet?.AddHeroTranscendStone(100)),
                new DebugPanelButtonDescriptor("Wood +5000", () => wallet?.AddWood(5000)),
                new DebugPanelButtonDescriptor("Brick +5000", () => wallet?.AddBrick(5000)),
                new DebugPanelButtonDescriptor("Iron +5000", () => wallet?.AddIron(5000)),
                new DebugPanelButtonDescriptor("Hero Ticket +10", () => wallet?.AddHeroSummonTicket(10)),
                new DebugPanelButtonDescriptor("Equip Ticket +10", () => wallet?.AddEquipmentSummonTicket(10)),
                new DebugPanelButtonDescriptor("Hero Lv +5", () => battleManager?.DebugLevelAllHeroes(5)),
                new DebugPanelButtonDescriptor("계정 EXP +50K", () => accountProgressManager?.AddExperience(GameNumber.FromDouble(50000))),
                new DebugPanelButtonDescriptor("계정 Lv +100", () => accountProgressManager?.DebugAddLevels(100)),
                new DebugPanelButtonDescriptor("특성P +1000", () => accountProgressManager?.DebugAddTalentPoints(1000)),
                new DebugPanelButtonDescriptor("Fortress EXP +50K", () => battleManager?.DebugAddFortressExperience(GameNumber.FromDouble(50000))),
                new DebugPanelButtonDescriptor("Fortress Lv +10", () => battleManager?.DebugLevelFortress(10)),
                new DebugPanelButtonDescriptor("Unlock Totems", () => battleManager?.DebugUnlockAllTotems()),
                new DebugPanelButtonDescriptor("Unlock Runes", () => battleManager?.DebugUnlockAllRunes()),
                new DebugPanelButtonDescriptor("Rune +100", () => battleManager?.DebugAddRuneItems(100)),
                new DebugPanelButtonDescriptor("Unlock 4x", () => speedManager?.DebugSetFourTimesEntitlement(true)),
                new DebugPanelButtonDescriptor("Unlock All", () => progressManager?.DebugUnlockThrough(GameData.ChapterOneBossStageId)),
                new DebugPanelButtonDescriptor("Facility 12h", () => battleManager?.DebugSimulateFacilityHours(12f)),
                new DebugPanelButtonDescriptor("Facility Lv +1", () => battleManager?.DebugLevelUpAllFacilities()),
                new DebugPanelButtonDescriptor("1-19 Repeat", () => progressManager?.DebugJumpToStage(GameData.BossFallbackStageId, ProgressMode.RepeatSelected)),
                new DebugPanelButtonDescriptor("1-20 Boss", () => progressManager?.DebugJumpToStage(GameData.ChapterOneBossStageId, ProgressMode.AutoProgress)),
                new DebugPanelButtonDescriptor("Reset Save", () => resetSaveAction?.Invoke(), new Color(0.45f, 0.16f, 0.14f, 1f), false)
            };
        }

        public static List<DebugPanelButtonDescriptor> BuildTimeButtons(Action<float> setTimeScale)
        {
            return new List<DebugPanelButtonDescriptor>
            {
                new DebugPanelButtonDescriptor("Time x1", () => setTimeScale?.Invoke(1f)),
                new DebugPanelButtonDescriptor("Time x5", () => setTimeScale?.Invoke(5f)),
                new DebugPanelButtonDescriptor("Time x20", () => setTimeScale?.Invoke(20f))
            };
        }

        public static string BuildStatusText(
            float timeScale,
            GameSpeedManager speedManager,
            CurrencyWallet wallet,
            AccountProgressManager accountProgressManager,
            StageProgressManager progressManager,
            BattleManager battleManager,
            Func<GameNumber, string> formatGameNumber,
            Func<long, string> formatCount)
        {
            string accountDebugText = accountProgressManager != null
                ? "\nAccount Lv: " + accountProgressManager.Level
                    + " EXP " + FormatGameNumber(accountProgressManager.Experience, formatGameNumber)
                    + "/" + FormatGameNumber(accountProgressManager.NextLevelExperience, formatGameNumber)
                    + " TP " + accountProgressManager.AvailableTalentPoints
                    + "/" + accountProgressManager.TotalTalentPointsEarned
                    + " Bonus " + accountProgressManager.DebugTalentPointBonus
                : string.Empty;

            return "Time Scale x" + timeScale.ToString("0.##")
                + "\nCombat Speed x" + (speedManager != null ? speedManager.CurrentMultiplier.ToString() : "0")
                + "\n4x Entitlement: " + (speedManager != null && speedManager.HasFourTimesSpeedEntitlement)
                + "\nTotem Essence: " + FormatCount(wallet != null ? wallet.TotemEssence : 0, formatCount)
                + accountDebugText
                + "\nOffline Reward Stage: " + (progressManager != null ? progressManager.GetOfflineRewardStageId() : string.Empty)
                + "\nBoss Cleared: " + (progressManager != null && progressManager.ChapterOneBossCleared)
                + "\nLast Battle: " + (battleManager != null ? battleManager.LastBattleLog : string.Empty);
        }

        private static string FormatGameNumber(GameNumber value, Func<GameNumber, string> format)
        {
            return format != null ? format(value) : value.ToString();
        }

        private static string FormatCount(long value, Func<long, string> format)
        {
            return format != null ? format(value) : value.ToString();
        }
    }
}
