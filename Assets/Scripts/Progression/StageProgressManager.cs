using System;
using System.Collections.Generic;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Save;

namespace IdleGame.Progression
{
    public sealed class StageProgressManager : MonoBehaviour
    {
        private SaveManager saveManager;

        public event Action Changed;

        public string CurrentStageId { get; private set; } = GameData.FirstStageId;
        public string HighestStageId { get; private set; } = GameData.FirstStageId;
        public string SelectedStageId { get; private set; } = GameData.FirstStageId;
        public ProgressMode Mode { get; private set; } = ProgressMode.AutoProgress;
        public bool ChapterOneBossCleared { get; private set; }

        public StageDefinition CurrentStage => GameData.GetStage(CurrentStageId);

        public void Initialize(SaveManager save)
        {
            saveManager = save;

            HighestStageId = saveManager.LoadString(SaveKeys.HighestStageId, GameData.FirstStageId);
            CurrentStageId = saveManager.LoadString(SaveKeys.CurrentStageId, GameData.FirstStageId);
            SelectedStageId = saveManager.LoadString(SaveKeys.SelectedStageId, GameData.FirstStageId);
            Mode = saveManager.LoadEnum(SaveKeys.ProgressMode, ProgressMode.AutoProgress);
            ChapterOneBossCleared = saveManager.LoadBool(SaveKeys.ChapterOneBossCleared, false);

            NormalizeState();
            SaveProgress();
            NotifyChanged();
        }

        public void HandleStageCleared()
        {
            StageDefinition clearedStage = CurrentStage;

            if (clearedStage.Type == StageType.Boss)
            {
                if (clearedStage.Id == GameData.ChapterOneBossStageId)
                {
                    ChapterOneBossCleared = true;
                }

                string nextStageId = GameData.GetNextStageId(clearedStage.Id);
                HighestStageId = GameData.MaxStageId(HighestStageId, string.IsNullOrEmpty(nextStageId) ? clearedStage.Id : nextStageId);
                if (Mode == ProgressMode.AutoProgress && !string.IsNullOrEmpty(nextStageId))
                {
                    CurrentStageId = nextStageId;
                    SelectedStageId = CurrentStageId;
                    Mode = ProgressMode.AutoProgress;
                }
                else
                {
                    CurrentStageId = GameData.GetPreviousNormalStageId(clearedStage.Id);
                    SelectedStageId = CurrentStageId;
                    Mode = ProgressMode.RepeatSelected;
                }

                SaveProgress();
                NotifyChanged();
                return;
            }

            if (Mode == ProgressMode.AutoProgress)
            {
                string nextStageId = GameData.GetNextStageId(clearedStage.Id);
                if (!string.IsNullOrEmpty(nextStageId))
                {
                    CurrentStageId = nextStageId;
                    HighestStageId = GameData.MaxStageId(HighestStageId, nextStageId);
                }
                else
                {
                    CurrentStageId = clearedStage.Id;
                }
            }
            else
            {
                CurrentStageId = string.IsNullOrEmpty(SelectedStageId) ? clearedStage.Id : SelectedStageId;
            }

            SaveProgress();
            NotifyChanged();
        }

        public void HandleBossFailed()
        {
            string fallbackStageId = CurrentStage.FailureStageId;
            if (string.IsNullOrEmpty(fallbackStageId))
            {
                fallbackStageId = GameData.GetPreviousNormalStageId(CurrentStageId);
            }

            CurrentStageId = fallbackStageId;
            SelectedStageId = fallbackStageId;
            Mode = ProgressMode.BossBlocked;

            SaveProgress();
            NotifyChanged();
        }

        public bool SelectStage(string stageId)
        {
            if (!GameData.IsStageUnlocked(stageId, HighestStageId))
            {
                return false;
            }

            CurrentStageId = stageId;
            SelectedStageId = stageId;
            Mode = ProgressMode.RepeatSelected;

            SaveProgress();
            NotifyChanged();
            return true;
        }

        public void ResumeAutoProgress()
        {
            Mode = ProgressMode.AutoProgress;
            CurrentStageId = HighestStageId;

            SaveProgress();
            NotifyChanged();
        }

        public void DebugJumpToStage(string stageId, ProgressMode mode)
        {
            StageDefinition stage = GameData.GetStage(stageId);
            CurrentStageId = stage.Id;
            SelectedStageId = stage.Id;
            HighestStageId = GameData.MaxStageId(HighestStageId, stage.Id);
            Mode = mode;

            SaveProgress();
            NotifyChanged();
        }

        public void DebugUnlockThrough(string stageId)
        {
            StageDefinition stage = GameData.GetStage(stageId);
            HighestStageId = GameData.MaxStageId(HighestStageId, stage.Id);

            SaveProgress();
            NotifyChanged();
        }

        public string GetOfflineRewardStageId()
        {
            StageDefinition stage = CurrentStage;
            return stage.Type == StageType.Boss ? GameData.GetPreviousNormalStageId(stage.Id) : stage.Id;
        }

        private void NormalizeState()
        {
            CurrentStageId = GameData.GetStage(CurrentStageId).Id;
            HighestStageId = GameData.GetStage(HighestStageId).Id;
            SelectedStageId = GameData.GetStage(SelectedStageId).Id;

            if (!GameData.IsStageUnlocked(CurrentStageId, HighestStageId))
            {
                CurrentStageId = HighestStageId;
            }

            if (!GameData.IsStageUnlocked(SelectedStageId, HighestStageId))
            {
                SelectedStageId = CurrentStageId;
            }
        }

        private void SaveProgress()
        {
            saveManager.SaveString(SaveKeys.HighestStageId, HighestStageId);
            saveManager.SaveString(SaveKeys.CurrentStageId, CurrentStageId);
            saveManager.SaveString(SaveKeys.SelectedStageId, SelectedStageId);
            saveManager.SaveString(SaveKeys.ProgressMode, Mode.ToString());
            saveManager.SaveBool(SaveKeys.ChapterOneBossCleared, ChapterOneBossCleared);
            saveManager.Flush();
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }
    }

    public enum DungeonKind
    {
        Ruby,
        Gold,
        TotemEssence,
        HeroTranscendStone
    }

    public readonly struct DungeonEntryReceipt
    {
        public DungeonEntryReceipt(DungeonKind kind, int level, bool usedFreeEntry, bool usedTicket)
        {
            Kind = kind;
            Level = level;
            UsedFreeEntry = usedFreeEntry;
            UsedTicket = usedTicket;
        }

        public DungeonKind Kind { get; }
        public int Level { get; }
        public bool UsedFreeEntry { get; }
        public bool UsedTicket { get; }
        public bool IsValid => UsedFreeEntry || UsedTicket;
    }

    public sealed class DungeonProgressManager : MonoBehaviour
    {
        public const int DailyFreeEntryLimit = 3;
        public const int RequiredNormalKills = 100;
        public const float DefaultTimeLimitSeconds = 30f;
        public const float GoldTimeLimitSeconds = 30f;
        public const float TotemBossTimeLimitSeconds = 30f;
        private const int RubyBaseReward = 300;
        private const int RubyRewardPerLevel = 5;

        private readonly Dictionary<DungeonKind, int> highestClearLevels = new Dictionary<DungeonKind, int>();
        private SaveManager saveManager;
        private CurrencyWallet wallet;
        private string freeEntryDate = string.Empty;
        private int freeEntriesUsed;

        public event Action Changed;

        public void Initialize(SaveManager save, CurrencyWallet currency)
        {
            saveManager = save;
            wallet = currency;
            Load();
            EnsureDailyReset();
            Save();
            NotifyChanged();
        }

        public int FreeEntriesRemaining
        {
            get
            {
                EnsureDailyReset();
                return Mathf.Max(0, DailyFreeEntryLimit - freeEntriesUsed);
            }
        }

        public int FreeEntriesUsed
        {
            get
            {
                EnsureDailyReset();
                return freeEntriesUsed;
            }
        }

        public bool CanEnter => FreeEntriesRemaining > 0 || (wallet != null && wallet.DungeonTicket > 0);

        public static string GetId(DungeonKind kind)
        {
            switch (kind)
            {
                case DungeonKind.Gold:
                    return "gold";
                case DungeonKind.TotemEssence:
                    return "totem";
                case DungeonKind.HeroTranscendStone:
                    return "transcend";
                default:
                    return "ruby";
            }
        }

        public static string GetTitle(DungeonKind kind)
        {
            switch (kind)
            {
                case DungeonKind.Gold:
                    return "골드 던전";
                case DungeonKind.TotemEssence:
                    return "토템석 던전";
                case DungeonKind.HeroTranscendStone:
                    return "초월석 던전";
                default:
                    return "보석 던전";
            }
        }

        public static bool HasSelectableLevel(DungeonKind kind)
        {
            return kind != DungeonKind.TotemEssence;
        }

        public static float GetTimeLimitSeconds(DungeonKind kind)
        {
            switch (kind)
            {
                case DungeonKind.Gold:
                    return GoldTimeLimitSeconds;
                case DungeonKind.TotemEssence:
                    return TotemBossTimeLimitSeconds;
                default:
                    return DefaultTimeLimitSeconds;
            }
        }

        public int GetHighestClearLevel(DungeonKind kind)
        {
            return highestClearLevels.TryGetValue(kind, out int level) ? Mathf.Max(0, level) : 0;
        }

        public int GetMaxSelectableLevel(DungeonKind kind)
        {
            if (!HasSelectableLevel(kind))
            {
                return 1;
            }

            return Mathf.Max(1, GetHighestClearLevel(kind) + 1);
        }

        public int ClampSelectableLevel(DungeonKind kind, int level)
        {
            if (!HasSelectableLevel(kind))
            {
                return 1;
            }

            return Mathf.Clamp(level, 1, GetMaxSelectableLevel(kind));
        }

        public bool CanSweep(DungeonKind kind, int level)
        {
            if (!HasSelectableLevel(kind))
            {
                return false;
            }

            return level >= 1 && level <= GetHighestClearLevel(kind) && CanEnter;
        }

        public bool TryConsumeEntry(DungeonKind kind, int level, out DungeonEntryReceipt receipt)
        {
            EnsureDailyReset();
            int normalizedLevel = ClampSelectableLevel(kind, level);
            if (FreeEntriesRemaining > 0)
            {
                freeEntriesUsed += 1;
                receipt = new DungeonEntryReceipt(kind, normalizedLevel, true, false);
                Save();
                NotifyChanged();
                return true;
            }

            if (wallet != null && wallet.SpendDungeonTicket(1))
            {
                receipt = new DungeonEntryReceipt(kind, normalizedLevel, false, true);
                Save();
                NotifyChanged();
                return true;
            }

            receipt = default;
            return false;
        }

        public void RefundEntry(DungeonEntryReceipt receipt)
        {
            if (!receipt.IsValid)
            {
                return;
            }

            EnsureDailyReset();
            if (receipt.UsedFreeEntry)
            {
                freeEntriesUsed = Mathf.Max(0, freeEntriesUsed - 1);
            }

            if (receipt.UsedTicket)
            {
                wallet?.AddDungeonTicket(1);
            }

            Save();
            NotifyChanged();
        }

        public string CompleteDungeon(DungeonKind kind, int level)
        {
            if (kind == DungeonKind.TotemEssence)
            {
                return CompleteTotemBossDungeon(level);
            }

            int normalizedLevel = Mathf.Max(1, level);
            if (normalizedLevel > GetHighestClearLevel(kind))
            {
                highestClearLevels[kind] = normalizedLevel;
            }

            string rewardText = GrantReward(kind, normalizedLevel);
            Save();
            NotifyChanged();
            return rewardText;
        }

        public string CompleteTotemBossDungeon(int defeatedBossLevel)
        {
            int normalizedLevel = Mathf.Max(0, defeatedBossLevel);
            if (normalizedLevel > GetHighestClearLevel(DungeonKind.TotemEssence))
            {
                highestClearLevels[DungeonKind.TotemEssence] = normalizedLevel;
            }

            long reward = GetTotemBossTotalReward(normalizedLevel);
            if (reward > 0)
            {
                wallet?.AddTotemEssence(reward);
            }

            Save();
            NotifyChanged();
            return normalizedLevel > 0
                ? "토템석 +" + reward.ToString("#,0")
                : "토템석 +0";
        }

        public string SweepDungeon(DungeonKind kind, int level)
        {
            int normalizedLevel = Mathf.Clamp(level, 1, GetHighestClearLevel(kind));
            return CompleteDungeon(kind, normalizedLevel);
        }

        public bool TrySweepDungeon(DungeonKind kind, int level, out string rewardText)
        {
            rewardText = string.Empty;
            int normalizedLevel = Mathf.Clamp(level, 1, GetHighestClearLevel(kind));
            if (!CanSweep(kind, normalizedLevel))
            {
                return false;
            }

            if (!TryConsumeEntry(kind, normalizedLevel, out DungeonEntryReceipt receipt))
            {
                return false;
            }

            rewardText = SweepDungeon(receipt.Kind, receipt.Level);
            return true;
        }

        public string GetRewardText(DungeonKind kind, int level)
        {
            int normalizedLevel = Mathf.Max(1, level);
            switch (kind)
            {
                case DungeonKind.Gold:
                    return NumberFormatter.Format(GetGoldReward(normalizedLevel));
                case DungeonKind.TotemEssence:
                    return "처치 수 누적";
                case DungeonKind.HeroTranscendStone:
                    return GetTranscendReward(normalizedLevel).ToString("#,0");
                default:
                    return GetRubyReward(normalizedLevel).ToString("#,0");
            }
        }

        private void Load()
        {
            highestClearLevels.Clear();
            foreach (DungeonKind kind in Enum.GetValues(typeof(DungeonKind)))
            {
                highestClearLevels[kind] = Mathf.Max(0, PlayerPrefs.GetInt(SaveKeys.DungeonHighestClearLevel(GetId(kind)), 0));
            }

            freeEntryDate = saveManager != null ? saveManager.LoadString(SaveKeys.DungeonFreeEntryDate, string.Empty) : string.Empty;
            freeEntriesUsed = saveManager != null ? Mathf.Clamp(PlayerPrefs.GetInt(SaveKeys.DungeonFreeEntriesUsed, 0), 0, DailyFreeEntryLimit) : 0;
        }

        private void Save()
        {
            if (saveManager == null)
            {
                return;
            }

            foreach (KeyValuePair<DungeonKind, int> pair in highestClearLevels)
            {
                PlayerPrefs.SetInt(SaveKeys.DungeonHighestClearLevel(GetId(pair.Key)), Mathf.Max(0, pair.Value));
            }

            saveManager.SaveString(SaveKeys.DungeonFreeEntryDate, freeEntryDate);
            PlayerPrefs.SetInt(SaveKeys.DungeonFreeEntriesUsed, Mathf.Clamp(freeEntriesUsed, 0, DailyFreeEntryLimit));
            saveManager.Flush();
        }

        private void EnsureDailyReset()
        {
            string today = DateTime.Now.ToString("yyyyMMdd");
            if (freeEntryDate == today)
            {
                return;
            }

            freeEntryDate = today;
            freeEntriesUsed = 0;
        }

        private string GrantReward(DungeonKind kind, int level)
        {
            switch (kind)
            {
                case DungeonKind.Gold:
                    GameNumber gold = GetGoldReward(level);
                    wallet?.AddGold(gold);
                    return "골드 +" + NumberFormatter.Format(gold);
                case DungeonKind.TotemEssence:
                    long totem = GetTotemReward(level);
                    wallet?.AddTotemEssence(totem);
                    return "토템석 +" + totem.ToString("#,0");
                case DungeonKind.HeroTranscendStone:
                    long transcend = GetTranscendReward(level);
                    wallet?.AddHeroTranscendStone(transcend);
                    return "초월석 +" + transcend.ToString("#,0");
                default:
                    long ruby = GetRubyReward(level);
                    wallet?.AddRuby(ruby);
                    return "보석 +" + ruby.ToString("#,0");
            }
        }

        private static long GetRubyReward(int level)
        {
            return RubyBaseReward + Mathf.Max(0, level - 1) * RubyRewardPerLevel;
        }

        private static GameNumber GetGoldReward(int level)
        {
            return GameNumber.FromDouble(1200d + Mathf.Max(0, level - 1) * 260d);
        }

        private static long GetTotemReward(int level)
        {
            return 80 + Mathf.Max(0, level - 1) * 4L;
        }

        public static long GetTotemBossTotalReward(int defeatedBossLevel)
        {
            long normalizedLevel = Math.Max(0, defeatedBossLevel);
            double reward = normalizedLevel * (2d * normalizedLevel + 78d);
            if (reward >= long.MaxValue)
            {
                return long.MaxValue;
            }

            return (long)reward;
        }

        private static long GetTranscendReward(int level)
        {
            return 12 + Mathf.Max(0, level - 1);
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }
    }
}
