using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Formation
{
    public sealed class HeroFormationViewRefs
    {
        public GameObject Content;
        public Text SummaryText;
        public Text OwnedEffectText;
        public RectTransform RosterGridRect;
    }

    public sealed class HeroRosterCardState
    {
        public bool IsOwned;
        public bool IsDeployed;
        public bool NeedsAttention;
        public bool ActionInteractable;
        public string DisplayText;
        public string ActionText;
        public Color ButtonColor;
        public Color ActionColor;
    }

    public sealed class HeroFormationSlotState
    {
        public bool Interactable;
        public bool RemoveVisible;
        public string Text;
        public Color TextColor;
        public Color ButtonColor;
    }

    public sealed class HeroFormationRuneSlotState
    {
        public bool Interactable;
        public bool RemoveVisible;
        public string Text;
        public Color ButtonColor;
    }

    public sealed class HeroFormationViewBuildArgs
    {
        public Transform Parent;
        public IReadOnlyList<HeroDefinition> RosterHeroes;
        public Func<HeroRarity, Color> GetRarityColor;
        public Action<int> OnFormationSlotClick;
        public Action<int> OnFormationSlotRemove;
        public Action<int> OnPresetClick;
        public Action<int> OnRuneSlotClick;
        public Action<int> OnRuneSlotRemove;
        public Action<string> OnHeroCardClick;
        public Action<string> OnHeroRosterActionClick;
        public Action OnAutoArrange;
        public Action OnBulkStarUp;
        public Dictionary<int, Button> PresetButtons;
        public Dictionary<int, Button> FormationSlotButtons;
        public Dictionary<int, Button> FormationSlotRemoveButtons;
        public Dictionary<int, Button> RuneSlotButtons;
        public Dictionary<int, Text> RuneSlotTexts;
        public Dictionary<int, Button> RuneSlotRemoveButtons;
        public Dictionary<string, Button> HeroRosterButtons;
        public Dictionary<string, Text> HeroButtonTexts;
        public Dictionary<string, Button> HeroRosterActionButtons;
        public Dictionary<string, GameObject> HeroRosterDeployedOverlays;
        public Dictionary<string, GameObject> HeroNotificationDots;
        public List<Text> FormationSlotTexts;
    }

    public static class HeroFormationView
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
            formationLayout.spacing = 6;
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
            formationAreaLayout.padding = new RectOffset(12, 12, 12, 12);
            formationAreaLayout.spacing = 10;
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
                22,
                FontStyle.Bold,
                TextAnchor.MiddleCenter);
            HudUiFactory.AddLayoutElement(refs.OwnedEffectText.gameObject, -1, HudLayoutConfig.HeroFormationOwnedEffectHeight);

            GameObject actionRow = new GameObject("HeroFormationActions", typeof(RectTransform));
            actionRow.transform.SetParent(content.transform, false);
            HorizontalLayoutGroup actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
            actionLayout.spacing = 14;
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

        private static void CreateFormationSlots(HeroFormationViewBuildArgs args, Transform parent)
        {
            GameObject slotGrid = new GameObject("FormationSlots", typeof(RectTransform));
            slotGrid.transform.SetParent(parent, false);
            GridLayoutGroup slotGridLayout = slotGrid.AddComponent<GridLayoutGroup>();
            slotGridLayout.cellSize = HudLayoutConfig.HeroFormationSlotCellSize;
            slotGridLayout.spacing = HudLayoutConfig.HeroFormationSlotSpacing;
            slotGridLayout.childAlignment = TextAnchor.MiddleCenter;
            slotGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            slotGridLayout.constraintCount = 4;
            float gridWidth = HudLayoutConfig.HeroFormationSlotCellSize.x * 4f
                + HudLayoutConfig.HeroFormationSlotSpacing.x * 3f;
            float gridHeight = HudLayoutConfig.HeroFormationSlotCellSize.y * 2f
                + HudLayoutConfig.HeroFormationSlotSpacing.y;
            HudUiFactory.AddLayoutElement(slotGrid, gridWidth, gridHeight);

            for (int i = 0; i < GameData.MaxPartyHeroes; i++)
            {
                int slotIndex = i;
                Button slot = HudUiFactory.CreateButton(string.Empty, slotGrid.transform, HudButtonStyle.Slot);
                slot.onClick.AddListener(() => args.OnFormationSlotClick?.Invoke(slotIndex));
                Text slotText = slot.GetComponentInChildren<Text>();
                slotText.name = "FormationSlotText" + i;
                slotText.fontSize = 17;
                slotText.alignment = TextAnchor.MiddleCenter;
                HudUiFactory.StretchToParent(slotText.gameObject);
                args.FormationSlotTexts.Add(slotText);
                args.FormationSlotButtons[i] = slot;

                Button removeButton = CreateCornerActionButton("-", slot.transform, HudButtonStyle.Danger);
                removeButton.onClick.AddListener(() => args.OnFormationSlotRemove?.Invoke(slotIndex));
                args.FormationSlotRemoveButtons[i] = removeButton;
            }
        }

        private static void CreatePresetColumn(HeroFormationViewBuildArgs args, Transform parent)
        {
            GameObject presetColumn = new GameObject("PresetColumn", typeof(RectTransform));
            presetColumn.transform.SetParent(parent, false);
            VerticalLayoutGroup presetLayout = presetColumn.AddComponent<VerticalLayoutGroup>();
            presetLayout.spacing = 6;
            presetLayout.childControlWidth = true;
            presetLayout.childControlHeight = true;
            presetLayout.childForceExpandWidth = true;
            presetLayout.childForceExpandHeight = false;
            HudUiFactory.AddLayoutElement(presetColumn, HudLayoutConfig.HeroPresetColumnWidth, -1);

            Text presetTitle = HudUiFactory.CreateText("PresetTitle", presetColumn.transform, HudLayoutConfig.HeroPresetTitleFontSize, FontStyle.Bold, TextAnchor.MiddleCenter);
            HudUiFactory.AddLayoutElement(presetTitle.gameObject, -1, HudLayoutConfig.HeroPresetTitleHeight);
            presetTitle.text = "프리셋";

            for (int preset = 1; preset <= GameData.MaxHeroPresets; preset++)
            {
                Button presetButton = HudUiFactory.CreateButton(preset.ToString(), presetColumn.transform, HudButtonStyle.SmallPreset);
                HudUiFactory.AddLayoutElement(presetButton.gameObject, -1, HudLayoutConfig.HeroPresetButtonHeight);
                int capturedPreset = preset;
                presetButton.onClick.AddListener(() => args.OnPresetClick?.Invoke(capturedPreset));
                args.PresetButtons[preset] = presetButton;
            }
        }

        private static void CreateRuneRow(HeroFormationViewBuildArgs args, Transform parent)
        {
            GameObject formationRuneRow = HudUiFactory.CreatePanel("FormationRuneRow", parent, new Color(0.27f, 0.35f, 0.50f, 1f));
            HorizontalLayoutGroup formationRuneLayout = formationRuneRow.AddComponent<HorizontalLayoutGroup>();
            formationRuneLayout.padding = new RectOffset(12, 12, 8, 8);
            formationRuneLayout.spacing = 10;
            formationRuneLayout.childControlWidth = true;
            formationRuneLayout.childControlHeight = true;
            formationRuneLayout.childForceExpandWidth = true;
            formationRuneLayout.childForceExpandHeight = true;
            HudUiFactory.AddLayoutElement(formationRuneRow, -1, HudLayoutConfig.HeroFormationRuneRowHeight);

            Text formationRuneTitle = HudUiFactory.CreateText("FormationRuneTitle", formationRuneRow.transform, 19, FontStyle.Bold, TextAnchor.MiddleCenter);
            HudUiFactory.AddLayoutElement(formationRuneTitle.gameObject, 60, -1);
            formationRuneTitle.text = "룬";

            for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
            {
                int capturedSlot = slot;
                Button runeSlotButton = HudUiFactory.CreateButton(string.Empty, formationRuneRow.transform, HudButtonStyle.RuneSlot);
                runeSlotButton.onClick.AddListener(() => args.OnRuneSlotClick?.Invoke(capturedSlot));
                args.RuneSlotButtons[capturedSlot] = runeSlotButton;
                args.RuneSlotTexts[capturedSlot] = runeSlotButton.GetComponentInChildren<Text>();

                Button removeButton = CreateCornerActionButton("-", runeSlotButton.transform, HudButtonStyle.Danger);
                removeButton.onClick.AddListener(() => args.OnRuneSlotRemove?.Invoke(capturedSlot));
                removeButton.gameObject.SetActive(false);
                args.RuneSlotRemoveButtons[capturedSlot] = removeButton;
            }
        }

        private static void CreateRoster(HeroFormationViewBuildArgs args, HeroFormationViewRefs refs, Transform parent)
        {
            IReadOnlyList<HeroDefinition> rosterHeroes = args.RosterHeroes ?? Array.Empty<HeroDefinition>();
            GameObject rosterScroll = HudUiFactory.CreatePanel("HeroRosterScroll", parent, new Color(0.15f, 0.19f, 0.28f, 1f));
            LayoutElement rosterScrollLayout = HudUiFactory.AddLayoutElement(rosterScroll, -1, HudLayoutConfig.HeroRosterMinHeight);
            rosterScrollLayout.flexibleHeight = 1f;

            ScrollRect rosterScrollRect = rosterScroll.AddComponent<ScrollRect>();
            rosterScrollRect.horizontal = false;
            rosterScrollRect.vertical = true;
            rosterScrollRect.movementType = ScrollRect.MovementType.Clamped;
            rosterScrollRect.inertia = false;
            rosterScrollRect.scrollSensitivity = 36f;

            GameObject rosterViewport = HudUiFactory.CreatePanel("HeroRosterViewport", rosterScroll.transform, new Color(0f, 0f, 0f, 0f));
            HudUiFactory.StretchToParent(rosterViewport);
            Image viewportImage = rosterViewport.GetComponent<Image>();
            if (viewportImage != null)
            {
                viewportImage.raycastTarget = true;
            }

            rosterViewport.AddComponent<RectMask2D>();
            rosterScrollRect.viewport = rosterViewport.GetComponent<RectTransform>();

            GameObject rosterGrid = new GameObject("HeroRosterGrid", typeof(RectTransform));
            rosterGrid.transform.SetParent(rosterViewport.transform, false);
            RectTransform rosterGridRect = rosterGrid.GetComponent<RectTransform>();
            refs.RosterGridRect = rosterGridRect;

            int rosterColumns = HudLayoutConfig.HeroRosterColumns;
            float rosterCellWidth = HudLayoutConfig.HeroRosterCardSize.x;
            float rosterCellHeight = HudLayoutConfig.HeroRosterCardSize.y;
            float rosterSpacingX = HudLayoutConfig.HeroRosterCardSpacing.x;
            float rosterSpacingY = HudLayoutConfig.HeroRosterCardSpacing.y;
            int rosterRows = Mathf.CeilToInt(rosterHeroes.Count / (float)rosterColumns);
            float rosterWidth = rosterColumns * rosterCellWidth + Mathf.Max(0, rosterColumns - 1) * rosterSpacingX;
            float rosterHeight = Mathf.Max(rosterCellHeight, rosterRows * rosterCellHeight + Mathf.Max(0, rosterRows - 1) * rosterSpacingY);
            rosterGridRect.anchorMin = new Vector2(0f, 1f);
            rosterGridRect.anchorMax = new Vector2(0f, 1f);
            rosterGridRect.pivot = new Vector2(0f, 1f);
            rosterGridRect.sizeDelta = new Vector2(rosterWidth, rosterHeight);
            rosterGridRect.anchoredPosition = new Vector2(8f, -4f);
            rosterScrollRect.content = rosterGridRect;
            rosterScrollRect.verticalNormalizedPosition = 1f;

            GridLayoutGroup rosterLayout = rosterGrid.AddComponent<GridLayoutGroup>();
            rosterLayout.cellSize = new Vector2(rosterCellWidth, rosterCellHeight);
            rosterLayout.spacing = new Vector2(rosterSpacingX, rosterSpacingY);
            rosterLayout.childAlignment = TextAnchor.UpperLeft;
            rosterLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            rosterLayout.constraintCount = rosterColumns;

            HeroRosterResponsiveGrid responsiveGrid = rosterGrid.AddComponent<HeroRosterResponsiveGrid>();
            responsiveGrid.Initialize(
                rosterScrollRect.viewport,
                rosterGridRect,
                rosterLayout,
                rosterColumns,
                rosterCellHeight / rosterCellWidth,
                new Vector2(rosterSpacingX, rosterSpacingY));

            foreach (HeroDefinition hero in rosterHeroes)
            {
                Color color = args.GetRarityColor != null ? args.GetRarityColor(hero.Rarity) : Color.gray;
                Button button = HudUiFactory.CreateButton(hero.DisplayName, rosterGrid.transform, HudLayoutConfig.HeroRosterCardFontSize, color);
                string heroId = hero.Id;
                button.onClick.AddListener(() => args.OnHeroCardClick?.Invoke(heroId));
                args.HeroRosterButtons[hero.Id] = button;
                args.HeroButtonTexts[hero.Id] = button.GetComponentInChildren<Text>();

                GameObject deployedOverlay = HudUiFactory.CreatePanel(hero.Id + "DeployedOverlay", button.transform, new Color(0f, 0f, 0f, 0.62f));
                HudUiFactory.StretchToParent(deployedOverlay);
                Image overlayImage = deployedOverlay.GetComponent<Image>();
                if (overlayImage != null)
                {
                    overlayImage.raycastTarget = false;
                }

                Text deployedText = HudUiFactory.CreateText(hero.Id + "DeployedOverlayText", deployedOverlay.transform, 21, FontStyle.Bold, TextAnchor.MiddleCenter);
                deployedText.color = new Color(1f, 0.92f, 0.42f, 1f);
                deployedText.text = "배치됨";
                deployedText.raycastTarget = false;
                HudUiFactory.StretchToParent(deployedText.gameObject);
                deployedOverlay.SetActive(false);
                args.HeroRosterDeployedOverlays[hero.Id] = deployedOverlay;

                Button actionButton = CreateCornerActionButton("+", button.transform, HudButtonStyle.ActionAdd);
                actionButton.onClick.AddListener(() => args.OnHeroRosterActionClick?.Invoke(heroId));
                args.HeroRosterActionButtons[hero.Id] = actionButton;
                args.HeroNotificationDots[hero.Id] = CreateNotificationDot(button.transform, 40f, new Vector2(-16f, -16f));
            }

        }

        private static Button CreateCornerActionButton(string label, Transform parent, HudButtonVisualStyle style)
        {
            Button button = HudUiFactory.CreateButton(label, parent, style);
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(34f, 34f);
            rect.anchoredPosition = new Vector2(-4f, -4f);
            return button;
        }

        private static GameObject CreateNotificationDot(Transform parent, float size, Vector2 anchoredPosition)
        {
            Text dot = HudUiFactory.CreateText("RedDot", parent, Mathf.RoundToInt(size), FontStyle.Bold, TextAnchor.MiddleCenter);
            dot.color = new Color(1f, 0.04f, 0.04f, 1f);
            dot.text = "●";
            dot.raycastTarget = false;

            RectTransform rect = dot.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = anchoredPosition;
            dot.gameObject.SetActive(false);
            return dot.gameObject;
        }
    }
}
