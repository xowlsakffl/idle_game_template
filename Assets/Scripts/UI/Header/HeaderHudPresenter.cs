using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Progression;

namespace IdleGame.UI.Header
{
    public sealed class HeaderHudPresenterArgs
    {
        public CurrencyWallet Wallet;
        public AccountProgressManager AccountProgressManager;
        public BattleManager BattleManager;
        public StageDefinition Stage;
        public Text ResourceText;
        public Text RubyResourceText;
        public Text StageText;
        public Text ModeText;
        public Text AccountLevelText;
        public Text FieldStagePillText;
        public Image AccountExpFill;
        public Func<GameNumber, string> FormatGameNumber;
        public Func<double, string> FormatDoubleNumber;
        public Func<long, string> FormatCountNumber;
    }

    public static class HeaderHudPresenter
    {
        public static void Refresh(HeaderHudPresenterArgs args)
        {
            if (args == null)
            {
                return;
            }

            if (args.ResourceText != null && args.Wallet != null)
            {
                args.ResourceText.text = Format(args, args.Wallet.Gold);
            }

            if (args.RubyResourceText != null && args.Wallet != null)
            {
                args.RubyResourceText.text = Format(args, args.Wallet.Ruby);
            }

            if (args.StageText != null)
            {
                args.StageText.text = "프로필 캐릭터";
            }

            if (args.ModeText != null && args.BattleManager != null)
            {
                args.ModeText.text = Format(args, args.BattleManager.TotalCombatPower);
            }

            RefreshAccountProgress(args);
            RefreshFieldStagePill(args);
        }

        private static void RefreshAccountProgress(HeaderHudPresenterArgs args)
        {
            AccountProgressManager progressManager = args.AccountProgressManager;
            if (progressManager == null)
            {
                return;
            }

            float accountExpRatio = Mathf.Clamp01((float)progressManager.Experience.RatioTo(progressManager.NextLevelExperience));
            if (args.AccountExpFill != null)
            {
                args.AccountExpFill.rectTransform.anchorMax = new Vector2(accountExpRatio, 1f);
            }

            if (args.AccountLevelText != null)
            {
                args.AccountLevelText.text = "계정 Lv." + progressManager.Level
                    + "  " + Format(args, progressManager.Experience)
                    + "/" + Format(args, progressManager.NextLevelExperience);
            }
        }

        private static void RefreshFieldStagePill(HeaderHudPresenterArgs args)
        {
            if (args.FieldStagePillText == null || args.BattleManager == null || args.Stage == null)
            {
                return;
            }

            args.FieldStagePillText.text = args.BattleManager.IsBossFight
                ? args.Stage.Id + " BOSS"
                : args.Stage.Id;
        }

        private static string Format(HeaderHudPresenterArgs args, GameNumber value)
        {
            return args.FormatGameNumber == null ? value.ToString() : args.FormatGameNumber(value);
        }

        private static string Format(HeaderHudPresenterArgs args, double value)
        {
            return args.FormatDoubleNumber == null ? value.ToString("0") : args.FormatDoubleNumber(value);
        }

        private static string Format(HeaderHudPresenterArgs args, long value)
        {
            return args.FormatCountNumber == null ? value.ToString("#,0") : args.FormatCountNumber(value);
        }
    }
}
