using System;
using UnityEngine;
using IdleGame.Data;
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
}
