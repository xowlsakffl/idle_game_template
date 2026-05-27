using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Progression;
using IdleGame.Speed;

namespace IdleGame.UI.Debugging
{
    public sealed class DebugPanelPresenterArgs
    {
        public bool RefreshPanel;
        public Text StatusText;
        public GameSpeedManager SpeedManager;
        public CurrencyWallet Wallet;
        public AccountProgressManager AccountProgressManager;
        public StageProgressManager ProgressManager;
        public BattleManager BattleManager;
        public Func<GameNumber, string> FormatShortNumber;
        public Func<long, string> FormatCountNumber;
    }

    public static class DebugPanelPresenter
    {
        public static void Refresh(DebugPanelPresenterArgs args)
        {
            if (args == null || !args.RefreshPanel || args.StatusText == null)
            {
                return;
            }

            args.StatusText.text = DebugPanelStateBuilder.BuildStatusText(
                Time.timeScale,
                args.SpeedManager,
                args.Wallet,
                args.AccountProgressManager,
                args.ProgressManager,
                args.BattleManager,
                value => Format(args, value),
                args.FormatCountNumber);
        }

        private static string Format(DebugPanelPresenterArgs args, GameNumber value)
        {
            return args.FormatShortNumber == null ? value.ToString() : args.FormatShortNumber(value);
        }
    }
}
