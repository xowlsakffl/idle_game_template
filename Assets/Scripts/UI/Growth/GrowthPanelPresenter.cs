using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Common;

namespace IdleGame.UI.Growth
{
    public sealed class GrowthPanelPresenterArgs
    {
        public AbilityManager AbilityManager;
        public CurrencyWallet Wallet;
        public int SelectedGrowthLevelStep;
        public bool RefreshPanel;
        public Dictionary<int, Button> GrowthStepButtons;
        public Dictionary<AbilityKind, Text> AbilityButtonTexts;
        public Dictionary<AbilityKind, Text> AbilityCostBadgeTexts;
        public Dictionary<AbilityKind, GameObject> AbilityNotificationDots;
        public Func<double, string> FormatShortNumber;
    }

    public static class GrowthPanelPresenter
    {
        public static bool Refresh(GrowthPanelPresenterArgs args)
        {
            if (args == null || args.AbilityManager == null || args.Wallet == null)
            {
                return false;
            }

            if (args.RefreshPanel)
            {
                RefreshStepButtons(args);
            }

            bool hasGrowthAttention = false;
            foreach (AbilityState ability in args.AbilityManager.States)
            {
                int cappedLevels = args.AbilityManager.GetCappedLevelCount(ability, args.SelectedGrowthLevelStep);
                long selectedCost = args.AbilityManager.GetLevelUpCost(ability, cappedLevels);
                bool canBuySelected = !ability.IsMaxed
                    && cappedLevels > 0
                    && selectedCost > 0
                    && selectedCost <= args.Wallet.Gold;
                bool canBuyOne = !ability.IsMaxed
                    && ability.LevelUpCost > 0
                    && ability.LevelUpCost <= args.Wallet.Gold;
                hasGrowthAttention |= canBuyOne;

                if (!args.RefreshPanel)
                {
                    continue;
                }

                RefreshAbilityRow(args, ability, selectedCost, canBuySelected);
            }

            return hasGrowthAttention;
        }

        private static void RefreshStepButtons(GrowthPanelPresenterArgs args)
        {
            if (args.GrowthStepButtons == null)
            {
                return;
            }

            foreach (KeyValuePair<int, Button> pair in args.GrowthStepButtons)
            {
                bool selected = pair.Key == args.SelectedGrowthLevelStep;
                HudUiFactory.ApplySpriteButtonState(
                    pair.Value,
                    HudSpriteKind.BlueMenuButton,
                    HudSpriteKind.BlueMenuButtonPressed,
                    selected);

                Text text = pair.Value != null ? pair.Value.GetComponentInChildren<Text>(true) : null;
                if (text != null)
                {
                    text.text = pair.Key + "x";
                    text.color = selected ? new Color(1f, 0.91f, 0.40f, 1f) : Color.white;
                }
            }
        }

        private static void RefreshAbilityRow(
            GrowthPanelPresenterArgs args,
            AbilityState ability,
            long selectedCost,
            bool canBuySelected)
        {
            if (args.AbilityButtonTexts == null
                || !args.AbilityButtonTexts.TryGetValue(ability.Definition.Kind, out Text text)
                || text == null)
            {
                return;
            }

            SetNotificationDot(args.AbilityNotificationDots, ability.Definition.Kind, canBuySelected);

            string costText = ability.IsMaxed ? "MAX" : "G " + FormatShortNumber(args, selectedCost);
            string levelText = ability.IsMaxed ? "MAX" : "Lv." + ability.Level + "/" + ability.Definition.MaxLevel;
            text.text = ability.Definition.DisplayName
                + "  " + levelText
                + "\n" + args.AbilityManager.GetDisplayValue(ability);

            Button rowButton = text.GetComponentInParent<Button>();
            if (rowButton != null)
            {
                HudUiFactory.ApplyButtonSprite(rowButton, HudSpriteKind.ParchmentPanel, ability.IsMaxed
                    ? new Color(0.58f, 0.68f, 0.78f, 1f)
                    : canBuySelected ? new Color(0.72f, 0.90f, 0.96f, 1f) : new Color(0.46f, 0.56f, 0.68f, 1f));
            }

            RefreshCostBadge(args, ability, costText, canBuySelected);
        }

        private static void RefreshCostBadge(
            GrowthPanelPresenterArgs args,
            AbilityState ability,
            string costText,
            bool canBuySelected)
        {
            if (args.AbilityCostBadgeTexts == null
                || !args.AbilityCostBadgeTexts.TryGetValue(ability.Definition.Kind, out Text costBadgeText)
                || costBadgeText == null)
            {
                return;
            }

            costBadgeText.text = costText;
            costBadgeText.color = ability.IsMaxed
                ? new Color(1f, 0.88f, 0.24f, 1f)
                : new Color(0.04f, 0.06f, 0.05f, 1f);

            Image badgeImage = costBadgeText.GetComponentInParent<Image>();
            if (badgeImage != null)
            {
                HudUiFactory.ApplySprite(badgeImage, ability.IsMaxed ? HudSpriteKind.DisabledPanel : HudSpriteKind.BigBlueButton,
                    ability.IsMaxed
                        ? new Color(0.58f, 0.64f, 0.72f, 1f)
                        : canBuySelected ? new Color(0.96f, 1f, 0.86f, 1f) : new Color(0.56f, 0.64f, 0.76f, 1f));
            }
        }

        private static void SetNotificationDot<TKey>(Dictionary<TKey, GameObject> dots, TKey key, bool visible)
        {
            if (dots != null && dots.TryGetValue(key, out GameObject dot) && dot != null)
            {
                dot.SetActive(visible);
            }
        }

        private static string FormatShortNumber(GrowthPanelPresenterArgs args, double value)
        {
            return args.FormatShortNumber != null ? args.FormatShortNumber(value) : NumberFormatter.Format(value);
        }
    }
}
