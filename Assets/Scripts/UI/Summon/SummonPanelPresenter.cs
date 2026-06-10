using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Gacha;
using IdleGame.UI.Common;

namespace IdleGame.UI.Summon
{
    public sealed class SummonPanelPresenterArgs
    {
        public GachaManager GachaManager;
        public CurrencyWallet Wallet;
        public SummonScreenViewRefs Refs;
        public GachaPoolKind SelectedPool;
        public string SelectedEventTargetId;
        public bool RefreshPanel;
        public bool ResultPopupOpen;
    }

    public static partial class SummonPanelPresenter
    {
        public static void Refresh(SummonPanelPresenterArgs args)
        {
            if (args == null || !args.RefreshPanel || args.Refs == null)
            {
                return;
            }

            SummonScreenViewRefs refs = args.Refs;
            GachaManager manager = args.GachaManager;
            GachaPoolKind selectedPool = args.SelectedPool;
            GachaPoolDefinition definition = manager != null
                ? manager.GetPoolDefinition(selectedPool)
                : GachaPoolDefinitions.Get(selectedPool);
            GachaPoolProgress progress = manager != null
                ? manager.GetProgress(selectedPool, args.SelectedEventTargetId)
                : new GachaPoolProgress(definition, 0, 0);

            RefreshPoolTabs(refs.PoolButtons, selectedPool);
            RefreshEventTargets(refs, selectedPool, args.SelectedEventTargetId);
            RefreshLevel(refs, progress);
            RefreshCurrency(refs, definition, args.Wallet);
            RefreshFeatured(refs, manager, definition, selectedPool, args.SelectedEventTargetId);
            RefreshPity(refs, progress);
            RefreshRollButtons(refs, manager, selectedPool);
            RefreshResult(refs, manager);
            RefreshResultPopup(refs, manager, selectedPool, args.ResultPopupOpen);
        }

        private static void RefreshEventTargets(SummonScreenViewRefs refs, GachaPoolKind selectedPool, string selectedEventTargetId)
        {
            bool visible = selectedPool == GachaPoolKind.Event;
            if (refs.EventTargetRoot != null)
            {
                refs.EventTargetRoot.SetActive(visible);
            }

            if (!visible || refs.EventTargetButtons == null)
            {
                return;
            }

            GachaEventTargetDefinition selectedTarget = GachaEventTargetDefinitions.Get(selectedEventTargetId);
            string selectedId = selectedTarget != null ? selectedTarget.Id : string.Empty;
            foreach (KeyValuePair<string, Button> pair in refs.EventTargetButtons)
            {
                bool selected = pair.Key == selectedId;
                HudUiFactory.ApplySpriteButtonState(
                    pair.Value,
                    selected ? HudSpriteKind.BigBlueButton : HudSpriteKind.BlueMenuButton,
                    HudSpriteKind.BlueMenuButtonPressed,
                    selected);
                ConfigureEventTargetButtonText(pair.Value);
            }
        }

        private static void RefreshPoolTabs(Dictionary<GachaPoolKind, Button> buttons, GachaPoolKind selectedPool)
        {
            if (buttons == null)
            {
                return;
            }

            foreach (KeyValuePair<GachaPoolKind, Button> pair in buttons)
            {
                bool selected = pair.Key == selectedPool;
                HudUiFactory.ApplySpriteButtonState(
                    pair.Value,
                    selected ? HudSpriteKind.BlueMenuButtonPressed : HudSpriteKind.BlueMenuButton,
                    HudSpriteKind.BlueMenuButtonPressed,
                    selected);
            }
        }

        private static void RefreshLevel(SummonScreenViewRefs refs, GachaPoolProgress progress)
        {
            if (refs.LevelText != null)
            {
                refs.LevelText.text = "Lv. " + progress.Level;
            }

            if (refs.LevelFill != null)
            {
                RectTransform fillRect = refs.LevelFill.rectTransform;
                fillRect.anchorMax = new Vector2(progress.LevelProgress01, 1f);
                fillRect.offsetMax = new Vector2(0f, fillRect.offsetMax.y);
            }

            if (refs.LevelProgressText != null)
            {
                refs.LevelProgressText.text = progress.IsMaxLevel
                    ? "MAX"
                    : progress.PullsIntoLevel + " / " + progress.PullsForNextLevel;
            }
        }

        private static void RefreshCurrency(SummonScreenViewRefs refs, GachaPoolDefinition definition, CurrencyWallet wallet)
        {
            if (refs.CurrencyText == null)
            {
                return;
            }

            long ruby = wallet != null ? wallet.Ruby : 0;
            if (definition.UsesHeroTicket)
            {
                long tickets = wallet != null ? wallet.HeroSummonTicket : 0;
                refs.CurrencyText.text = "영웅권 " + tickets.ToString("N0") + " / 루비 " + ruby.ToString("N0");
                return;
            }

            if (definition.UsesEquipmentTicket)
            {
                long tickets = wallet != null ? wallet.EquipmentSummonTicket : 0;
                refs.CurrencyText.text = "장비권 " + tickets.ToString("N0") + " / 루비 " + ruby.ToString("N0");
                return;
            }

            refs.CurrencyText.text = "루비 " + ruby.ToString("N0");
        }

        private static void RefreshFeatured(SummonScreenViewRefs refs, GachaManager manager, GachaPoolDefinition definition, GachaPoolKind selectedPool, string selectedEventTargetId)
        {
            if (refs.TitleText != null)
            {
                refs.TitleText.text = "소환";
            }

            if (refs.FeaturedBadgeText != null)
            {
                refs.FeaturedBadgeText.text = selectedPool == GachaPoolKind.Event ? "이벤트" : "확률 상승";
            }

            if (refs.FeaturedTitleText != null)
            {
                refs.FeaturedTitleText.text = manager != null
                    ? manager.GetFeaturedRewardText(selectedPool, selectedEventTargetId)
                    : definition.FeaturedLabel;
            }

            if (refs.FeaturedDescriptionText != null)
            {
                refs.FeaturedDescriptionText.text = selectedPool == GachaPoolKind.Event
                    ? BuildEventDescription(selectedEventTargetId)
                    : definition.Title + "\n" + definition.Description;
            }

            if (refs.RateText != null)
            {
                refs.RateText.text = manager != null
                    ? manager.GetRateSummaryText(selectedPool, selectedEventTargetId)
                    : GachaRateTable.GetRateSummaryText(1);
            }
        }

        private static string BuildEventDescription(string selectedEventTargetId)
        {
            GachaEventTargetDefinition target = GachaEventTargetDefinitions.Get(selectedEventTargetId);
            if (target == null)
            {
                return "이벤트 소환\n이벤트 대상을 선택";
            }

            return target.CategoryLabel == "영웅"
                ? "이벤트 소환\n선택 영웅 확률 상승 / 500회 신화 확정"
                : "이벤트 소환\n선택 장비 확률 상승";
        }

        private static void RefreshPity(SummonScreenViewRefs refs, GachaPoolProgress progress)
        {
            if (refs.PityRoot != null)
            {
                refs.PityRoot.SetActive(progress.HasPity);
            }

            if (!progress.HasPity)
            {
                return;
            }

            if (refs.PityFill != null)
            {
                RectTransform fillRect = refs.PityFill.rectTransform;
                fillRect.anchorMax = new Vector2(progress.PityProgress01, 1f);
                fillRect.offsetMax = new Vector2(0f, fillRect.offsetMax.y);
            }

            if (refs.PityText != null)
            {
                refs.PityText.text = progress.PityCount + " / " + progress.PityLimit;
            }
        }

        private static void RefreshRollButtons(SummonScreenViewRefs refs, GachaManager manager, GachaPoolKind selectedPool)
        {
            if (refs.RollOneButton != null)
            {
                bool canRoll = manager != null && manager.CanRoll(selectedPool, 1);
                refs.RollOneButton.interactable = canRoll;
                HudUiFactory.SetButtonText(refs.RollOneButton, "1회 소환\n" + (manager != null ? manager.GetCostText(selectedPool, 1) : string.Empty));
                ConfigureButtonText(refs.RollOneButton);
            }

            if (refs.RollTenButton != null)
            {
                bool canRoll = manager != null && manager.CanRoll(selectedPool, 10);
                refs.RollTenButton.interactable = canRoll;
                HudUiFactory.SetButtonText(refs.RollTenButton, "10회 소환\n" + (manager != null ? manager.GetCostText(selectedPool, 10) : string.Empty));
                ConfigureButtonText(refs.RollTenButton);
            }
        }

        private static void RefreshResult(SummonScreenViewRefs refs, GachaManager manager)
        {
            if (refs.ResultText != null)
            {
                refs.ResultText.text = manager != null ? manager.LastResult : string.Empty;
            }
        }

        private static void RefreshResultPopup(SummonScreenViewRefs refs, GachaManager manager, GachaPoolKind selectedPool, bool resultPopupOpen)
        {
            bool hasResults = manager != null && manager.LastOutcomes.Count > 0;
            bool open = resultPopupOpen && hasResults;
            if (refs.ResultPopupRoot != null)
            {
                refs.ResultPopupRoot.SetActive(open);
            }

            if (!open)
            {
                return;
            }

            IReadOnlyList<GachaRollOutcome> outcomes = manager.LastOutcomes;
            for (int i = 0; i < refs.ResultPopupCards.Count; i++)
            {
                bool visible = i < outcomes.Count;
                GameObject card = refs.ResultPopupCards[i];
                if (card != null)
                {
                    card.SetActive(visible);
                }

                if (!visible)
                {
                    continue;
                }

                GachaRollOutcome outcome = outcomes[i];
                Image cardImage = card.GetComponent<Image>();
                if (cardImage != null)
                {
                    HudUiFactory.ApplySprite(cardImage, HudSpriteKind.SmallBlueSquareButton, GetRarityCardColor(outcome.Rarity));
                }

                if (i < refs.ResultPopupCardTexts.Count && refs.ResultPopupCardTexts[i] != null)
                {
                    refs.ResultPopupCardTexts[i].text = BuildResultCardText(outcome);
                }
            }

            RefreshResultPopupRollButton(refs.ResultPopupRollOneButton, manager, selectedPool, 1);
            RefreshResultPopupRollButton(refs.ResultPopupRollTenButton, manager, selectedPool, 10);
            ConfigureButtonText(refs.ResultPopupCloseButton);
        }

        private static void RefreshResultPopupRollButton(Button button, GachaManager manager, GachaPoolKind selectedPool, int count)
        {
            if (button == null)
            {
                return;
            }

            bool canRoll = manager != null && manager.CanRoll(selectedPool, count);
            button.interactable = canRoll;
            HudUiFactory.SetButtonText(button, count + "회 소환\n" + (manager != null ? manager.GetCostText(selectedPool, count) : string.Empty));
            ConfigureButtonText(button);
        }

        private static string BuildResultCardText(GachaRollOutcome outcome)
        {
            string tag = outcome.PityForced ? "확정 " : outcome.Pickup ? "이벤트 " : string.Empty;
            return tag
                + GachaRateTable.GetRarityLabel(outcome.Rarity)
                + "\n"
                + outcome.CategoryLabel
                + "\n"
                + outcome.DisplayName
                + "\nx"
                + outcome.Amount;
        }

        private static Color GetRarityCardColor(HeroRarity rarity)
        {
            switch (rarity)
            {
                case HeroRarity.Mythic:
                    return new Color(0.86f, 0.25f, 1f, 1f);
                case HeroRarity.Legendary:
                    return new Color(1f, 0.62f, 0.12f, 1f);
                case HeroRarity.Epic:
                    return new Color(0.62f, 0.30f, 0.92f, 1f);
                case HeroRarity.Rare:
                    return new Color(0.18f, 0.58f, 0.94f, 1f);
                case HeroRarity.Uncommon:
                    return new Color(0.30f, 0.72f, 0.25f, 1f);
                default:
                    return new Color(0.62f, 0.68f, 0.76f, 1f);
            }
        }

        private static void ConfigureButtonText(Button button)
        {
            Text text = button != null ? button.GetComponentInChildren<Text>(true) : null;
            if (text == null)
            {
                return;
            }

            text.lineSpacing = 0.82f;
            HudUiFactory.ConfigureBestFitText(text, 13, 23, 0.82f);
        }

        private static void ConfigureEventTargetButtonText(Button button)
        {
            Text text = button != null ? button.GetComponentInChildren<Text>(true) : null;
            if (text == null)
            {
                return;
            }

            text.lineSpacing = 0.78f;
            HudUiFactory.ConfigureBestFitText(text, 11, 17, 0.78f);
        }
    }
}
