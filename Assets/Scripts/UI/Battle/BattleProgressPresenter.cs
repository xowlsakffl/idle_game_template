using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;

namespace IdleGame.UI.Battle
{
    public sealed class BattleProgressPresenterArgs
    {
        public BattleManager BattleManager;
        public StageDefinition Stage;
        public Text TargetText;
        public Image ProgressFill;
        public Text ProgressValueText;
        public Text ProgressText;
        public Text GuideQuestText;
    }

    public static class BattleProgressPresenter
    {
        private static readonly Color BossProgressColor = new Color(0.90f, 0.18f, 0.16f, 1f);
        private static readonly Color KillProgressColor = new Color(0.95f, 0.63f, 0.17f, 1f);

        public static void Refresh(BattleProgressPresenterArgs args)
        {
            if (args == null || args.BattleManager == null)
            {
                return;
            }

            if (args.TargetText != null)
            {
                args.TargetText.gameObject.SetActive(false);
            }

            float progressRatio = CalculateProgressRatio(args.BattleManager);
            RefreshProgressBar(args, progressRatio);
            RefreshProgressText(args, progressRatio);
            RefreshGuideQuest(args);
        }

        private static float CalculateProgressRatio(BattleManager battleManager)
        {
            if (battleManager.IsBossFight)
            {
                return battleManager.TargetMaxHp <= GameNumber.Zero
                    ? 0f
                    : Mathf.Clamp01(1f - (float)battleManager.TargetHp.RatioTo(battleManager.TargetMaxHp));
            }

            return battleManager.RequiredKills <= 0
                ? 0f
                : Mathf.Clamp01((float)battleManager.KillsThisStage / battleManager.RequiredKills);
        }

        private static void RefreshProgressBar(BattleProgressPresenterArgs args, float progressRatio)
        {
            if (args.ProgressFill == null)
            {
                return;
            }

            args.ProgressFill.rectTransform.anchorMax = new Vector2(progressRatio, 1f);
            args.ProgressFill.color = args.BattleManager.IsBossFight ? BossProgressColor : KillProgressColor;
        }

        private static void RefreshProgressText(BattleProgressPresenterArgs args, float progressRatio)
        {
            BattleManager battleManager = args.BattleManager;
            if (battleManager.IsBossFight)
            {
                if (args.ProgressValueText != null)
                {
                    args.ProgressValueText.text = "BOSS " + Mathf.RoundToInt(progressRatio * 100f) + "%";
                }

                if (args.ProgressText != null)
                {
                    args.ProgressText.text = "Boss Timer: " + Mathf.CeilToInt(battleManager.BossTimeRemaining) + "s";
                }

                return;
            }

            if (args.ProgressValueText != null)
            {
                args.ProgressValueText.text = battleManager.KillsThisStage + " / " + battleManager.RequiredKills;
            }

            if (args.ProgressText != null)
            {
                args.ProgressText.text = "100마리 처치";
            }
        }

        private static void RefreshGuideQuest(BattleProgressPresenterArgs args)
        {
            if (args.GuideQuestText == null || args.Stage == null)
            {
                return;
            }

            BattleManager battleManager = args.BattleManager;
            string questGoal = battleManager.IsBossFight
                ? "보스 처치"
                : "스테이지 " + args.Stage.Id + " 클리어";
            string questProgress = battleManager.IsBossFight
                ? Mathf.CeilToInt(battleManager.BossTimeRemaining) + "초 남음"
                : battleManager.KillsThisStage + "/" + battleManager.RequiredKills;

            args.GuideQuestText.text = "가이드 퀘스트\n" + questGoal + "  " + questProgress;
        }
    }
}
