using System;
using UnityEngine.UI;
using IdleGame.Battle;

namespace IdleGame.UI.Hud
{
    public sealed class HudStatusPresenterArgs
    {
        public BattleManager BattleManager;
        public Text TotalCombatPowerText;
        public Text GrowthNoticeText;
        public string GrowthNoticeMessage;
        public float GrowthNoticeUntil;
        public float CurrentTime;
        public Func<double, string> FormatShortNumber;
    }

    public static class HudStatusPresenter
    {
        public static void Refresh(HudStatusPresenterArgs args)
        {
            if (args == null)
            {
                return;
            }

            if (args.TotalCombatPowerText != null && args.BattleManager != null)
            {
                args.TotalCombatPowerText.text = "종합 전투력 " + Format(args, args.BattleManager.TotalCombatPower);
            }

            if (args.GrowthNoticeText != null)
            {
                args.GrowthNoticeText.text = args.CurrentTime < args.GrowthNoticeUntil
                    ? args.GrowthNoticeMessage
                    : string.Empty;
            }
        }

        private static string Format(HudStatusPresenterArgs args, double value)
        {
            return args.FormatShortNumber == null ? value.ToString("0") : args.FormatShortNumber(value);
        }
    }
}
