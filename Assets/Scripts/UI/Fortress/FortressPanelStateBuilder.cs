using System;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;

namespace IdleGame.UI.Fortress
{
    public sealed class FortressPanelViewState
    {
        public string SummaryText;
        public string ExpText;
        public float ExpFillRatio;
        public string StatsText;
        public bool LevelUpInteractable;
        public string LevelUpText;
        public Color LevelUpColor;
    }

    public static class FortressPanelStateBuilder
    {
        private static readonly Color MaxButtonColor = new Color(0.20f, 0.24f, 0.32f, 1f);
        private static readonly Color CanLevelButtonColor = new Color(0.58f, 0.84f, 0.20f, 1f);
        private static readonly Color CannotLevelButtonColor = new Color(0.30f, 0.33f, 0.38f, 1f);

        public static FortressPanelViewState Build(
            BattleManager battleManager,
            Func<GameNumber, string> formatGameNumber,
            Func<double, string> formatDouble)
        {
            if (battleManager == null)
            {
                return null;
            }

            GameNumber currentRequired = battleManager.FortressCurrentLevelExperience;
            GameNumber nextRequired = battleManager.FortressNextLevelExperience;
            GameNumber exp = battleManager.FortressExperience;
            GameNumber currentProgress = GameNumber.Max(GameNumber.Zero, exp - currentRequired);
            GameNumber requiredSpan = GameNumber.Max(GameNumber.One, nextRequired - currentRequired);
            bool isMaxed = battleManager.FortressLevel >= battleManager.FortressMaxLevel;
            float expRatio = isMaxed ? 1f : Mathf.Clamp01((float)currentProgress.RatioTo(requiredSpan));

            return new FortressPanelViewState
            {
                SummaryText = "Lv." + battleManager.FortressLevel + "/" + battleManager.FortressMaxLevel
                    + "    전투력 " + FormatDouble(battleManager.FortressCombatPower, formatDouble)
                    + "\nHP " + FormatGameNumber(battleManager.FortressHp, formatGameNumber)
                    + "/" + FormatGameNumber(battleManager.FortressMaxHp, formatGameNumber),
                ExpText = isMaxed ? "MAX" : FormatGameNumber(currentProgress, formatGameNumber) + " / " + FormatGameNumber(requiredSpan, formatGameNumber),
                ExpFillRatio = expRatio,
                StatsText = "자동 공격 " + FormatGameNumber(battleManager.FortressAttackPower, formatGameNumber)
                    + "    간격 " + battleManager.FortressAttackInterval.ToString("0.00") + "초"
                    + "\n사거리 " + battleManager.FortressAttackRange.ToString("0.00")
                    + "    다음 레벨 필요 누적 EXP " + FormatGameNumber(nextRequired, formatGameNumber),
                LevelUpInteractable = !isMaxed,
                LevelUpText = isMaxed ? "MAX" : "레벨업",
                LevelUpColor = isMaxed ? MaxButtonColor
                    : battleManager.CanLevelUpFortress ? CanLevelButtonColor : CannotLevelButtonColor
            };
        }

        private static string FormatGameNumber(GameNumber value, Func<GameNumber, string> format)
        {
            return format != null ? format(value) : value.ToString();
        }

        private static string FormatDouble(double value, Func<double, string> format)
        {
            return format != null ? format(value) : value.ToString("0");
        }
    }
}
