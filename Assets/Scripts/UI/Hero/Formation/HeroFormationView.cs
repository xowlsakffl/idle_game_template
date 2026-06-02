using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Formation
{
    public static partial class HeroFormationView
    {
        public static HeroFormationViewRefs Build(HeroFormationViewBuildArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            HeroFormationViewRefs refs = new HeroFormationViewRefs();

            GameObject content = new GameObject("HeroFormationContent", typeof(RectTransform));
            content.transform.SetParent(args.Parent, false);
            refs.Content = content;

            VerticalLayoutGroup formationLayout = content.AddComponent<VerticalLayoutGroup>();
            formationLayout.spacing = 4;
            formationLayout.childControlWidth = true;
            formationLayout.childControlHeight = true;
            formationLayout.childForceExpandWidth = true;
            formationLayout.childForceExpandHeight = false;
            LayoutElement contentLayout = HudUiFactory.AddLayoutElement(content, -1, 0);
            contentLayout.flexibleHeight = 1f;

            refs.SummaryText = HudUiFactory.CreateText(
                "HeroFormationSummary",
                content.transform,
                HudLayoutConfig.HeroFormationSummaryFontSize,
                FontStyle.Bold,
                TextAnchor.MiddleLeft);
            HudUiFactory.AddLayoutElement(refs.SummaryText.gameObject, -1, HudLayoutConfig.HeroFormationSummaryHeight);

            GameObject formationArea = HudUiFactory.CreatePanel("FormationArea", content.transform, new Color(0.33f, 0.42f, 0.58f, 1f));
            HudUiFactory.AddLayoutElement(formationArea, -1, HudLayoutConfig.HeroFormationAreaHeight);
            HorizontalLayoutGroup formationAreaLayout = formationArea.AddComponent<HorizontalLayoutGroup>();
            formationAreaLayout.padding = new RectOffset(10, 10, 10, 10);
            formationAreaLayout.spacing = 8;
            formationAreaLayout.childAlignment = TextAnchor.MiddleCenter;
            formationAreaLayout.childControlWidth = true;
            formationAreaLayout.childControlHeight = true;
            formationAreaLayout.childForceExpandWidth = false;
            formationAreaLayout.childForceExpandHeight = true;

            CreateFormationSlots(args, formationArea.transform);
            CreatePresetColumn(args, formationArea.transform);
            CreateRuneRow(args, content.transform);
            CreateRoster(args, refs, content.transform);

            refs.OwnedEffectText = HudUiFactory.CreateText(
                "HeroOwnedEffect",
                content.transform,
                20,
                FontStyle.Bold,
                TextAnchor.MiddleCenter);
            HudUiFactory.AddLayoutElement(refs.OwnedEffectText.gameObject, -1, HudLayoutConfig.HeroFormationOwnedEffectHeight);

            GameObject actionRow = new GameObject("HeroFormationActions", typeof(RectTransform));
            actionRow.transform.SetParent(content.transform, false);
            HorizontalLayoutGroup actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
            actionLayout.spacing = 8;
            actionLayout.childControlWidth = true;
            actionLayout.childControlHeight = true;
            actionLayout.childForceExpandWidth = true;
            actionLayout.childForceExpandHeight = true;
            HudUiFactory.AddLayoutElement(actionRow, -1, HudLayoutConfig.HeroFormationActionRowHeight);

            Button autoArrangeButton = HudUiFactory.CreateButton("자동 배치", actionRow.transform, HudButtonStyle.Primary);
            autoArrangeButton.onClick.AddListener(() => args.OnAutoArrange?.Invoke());
            Button bulkStarUpButton = HudUiFactory.CreateButton("일괄 승급", actionRow.transform, HudButtonStyle.Secondary);
            bulkStarUpButton.onClick.AddListener(() => args.OnBulkStarUp?.Invoke());

            return refs;
        }

        public static void ApplyContentVisibility(
            GameObject formationContent,
            GameObject traitContent,
            GameObject totemContent,
            GameObject runeContent,
            Text placeholderText,
            bool formationOpen,
            bool traitOpen,
            bool totemOpen,
            bool runeOpen,
            string placeholderMessage)
        {
            if (formationContent != null)
            {
                formationContent.SetActive(formationOpen);
            }

            if (traitContent != null)
            {
                traitContent.SetActive(traitOpen);
            }

            if (totemContent != null)
            {
                totemContent.SetActive(totemOpen);
            }

            if (runeContent != null)
            {
                runeContent.SetActive(runeOpen);
            }

            if (placeholderText != null)
            {
                bool placeholderOpen = !formationOpen && !traitOpen && !totemOpen && !runeOpen;
                placeholderText.gameObject.SetActive(placeholderOpen);
                if (placeholderOpen)
                {
                    placeholderText.text = placeholderMessage ?? string.Empty;
                }
            }
        }

        public static void ApplySelectedButtonColors<TKey>(
            Dictionary<TKey, Button> buttons,
            TKey selectedKey,
            Color selectedColor,
            Color normalColor)
        {
            if (buttons == null)
            {
                return;
            }

            EqualityComparer<TKey> comparer = EqualityComparer<TKey>.Default;
            foreach (KeyValuePair<TKey, Button> pair in buttons)
            {
                HudUiFactory.SetButtonColor(pair.Value, comparer.Equals(pair.Key, selectedKey) ? selectedColor : normalColor);
            }
        }

        public static void ApplyFormationSlotState(
            int slotIndex,
            HeroFormationSlotState state,
            List<Text> slotTexts,
            Dictionary<int, Button> slotButtons,
            Dictionary<int, Button> removeButtons)
        {
            if (state == null)
            {
                return;
            }

            if (slotButtons != null && slotButtons.TryGetValue(slotIndex, out Button slotButton) && slotButton != null)
            {
                slotButton.interactable = state.Interactable;
                HudUiFactory.SetButtonColor(slotButton, state.ButtonColor);
            }

            if (removeButtons != null && removeButtons.TryGetValue(slotIndex, out Button removeButton) && removeButton != null)
            {
                removeButton.gameObject.SetActive(state.RemoveVisible);
            }

            if (slotTexts != null && slotIndex >= 0 && slotIndex < slotTexts.Count)
            {
                Text slotText = slotTexts[slotIndex];
                if (slotText != null)
                {
                    slotText.text = state.Text ?? string.Empty;
                    slotText.color = state.TextColor;
                }
            }
        }

        public static void ApplyRuneSlotState(
            int slot,
            HeroFormationRuneSlotState state,
            Dictionary<int, Text> slotTexts,
            Dictionary<int, Button> slotButtons,
            Dictionary<int, Button> removeButtons)
        {
            if (state == null)
            {
                return;
            }

            if (slotTexts != null && slotTexts.TryGetValue(slot, out Text text) && text != null)
            {
                text.text = state.Text ?? string.Empty;
            }

            if (slotButtons != null && slotButtons.TryGetValue(slot, out Button button) && button != null)
            {
                button.interactable = state.Interactable;
                HudUiFactory.SetButtonColor(button, state.ButtonColor);
            }

            if (removeButtons != null && removeButtons.TryGetValue(slot, out Button removeButton) && removeButton != null)
            {
                removeButton.gameObject.SetActive(state.RemoveVisible);
            }
        }

        public static void ApplyRosterCardState(
            string heroId,
            HeroRosterCardState state,
            Dictionary<string, Button> rosterButtons,
            Dictionary<string, Text> heroTexts,
            Dictionary<string, Button> actionButtons,
            Dictionary<string, GameObject> deployedOverlays,
            Dictionary<string, GameObject> notificationDots)
        {
            if (string.IsNullOrEmpty(heroId) || state == null)
            {
                return;
            }

            if (rosterButtons != null && rosterButtons.TryGetValue(heroId, out Button rosterButton) && rosterButton != null)
            {
                rosterButton.gameObject.SetActive(state.IsOwned);
                HudUiFactory.SetButtonColor(rosterButton, state.ButtonColor);
            }

            if (heroTexts != null && heroTexts.TryGetValue(heroId, out Text text) && text != null)
            {
                text.text = state.DisplayText ?? string.Empty;
            }

            if (deployedOverlays != null && deployedOverlays.TryGetValue(heroId, out GameObject deployedOverlay) && deployedOverlay != null)
            {
                deployedOverlay.SetActive(state.IsDeployed);
            }

            if (actionButtons != null && actionButtons.TryGetValue(heroId, out Button actionButton) && actionButton != null)
            {
                actionButton.interactable = state.ActionInteractable;
                Text actionTextComponent = actionButton.GetComponentInChildren<Text>(true);
                if (actionTextComponent != null)
                {
                    actionTextComponent.text = state.ActionText ?? string.Empty;
                }

                HudUiFactory.SetButtonColor(actionButton, state.ActionColor);
            }

            if (notificationDots != null && notificationDots.TryGetValue(heroId, out GameObject dot) && dot != null)
            {
                dot.SetActive(state.NeedsAttention);
            }
        }

    }
}
