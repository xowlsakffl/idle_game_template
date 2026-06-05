using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Formation
{
    public static partial class HeroFormationView
    {
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
            LayoutElement slotGridElement = HudUiFactory.AddLayoutElement(slotGrid, gridWidth, gridHeight);
            slotGridElement.flexibleWidth = 1f;
            slotGridElement.flexibleHeight = 1f;

            HeroFormationSlotGridSizer slotGridSizer = slotGrid.AddComponent<HeroFormationSlotGridSizer>();
            slotGridSizer.Initialize(
                slotGridLayout,
                4,
                2,
                HudLayoutConfig.HeroFormationSlotCellSize.y / HudLayoutConfig.HeroFormationSlotCellSize.x,
                HudLayoutConfig.HeroFormationSlotSpacing);

            for (int i = 0; i < GameData.MaxPartyHeroes; i++)
            {
                int slotIndex = i;
                Button slot = HudUiFactory.CreateButton(string.Empty, slotGrid.transform, HudButtonStyle.Slot);
                slot.onClick.AddListener(() => args.OnFormationSlotClick?.Invoke(slotIndex));
                Text slotText = slot.GetComponentInChildren<Text>();
                slotText.name = "FormationSlotText" + i;
                slotText.fontSize = 18;
                slotText.alignment = TextAnchor.MiddleCenter;
                HudUiFactory.ConfigureBestFitText(slotText, 13, 18, 0.88f);
                HudUiFactory.StretchToParent(slotText.gameObject);
                args.FormationSlotTexts.Add(slotText);
                args.FormationSlotButtons[i] = slot;

                Button removeButton = HudUiFactory.CreateCornerActionButton("-", slot.transform, HudButtonStyle.Danger);
                removeButton.onClick.AddListener(() => args.OnFormationSlotRemove?.Invoke(slotIndex));
                args.FormationSlotRemoveButtons[i] = removeButton;
            }
        }

        private static void CreatePresetColumn(HeroFormationViewBuildArgs args, Transform parent)
        {
            GameObject presetColumn = new GameObject("PresetColumn", typeof(RectTransform));
            presetColumn.transform.SetParent(parent, false);
            VerticalLayoutGroup presetLayout = presetColumn.AddComponent<VerticalLayoutGroup>();
            presetLayout.spacing = 5;
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
                Text presetText = presetButton.GetComponentInChildren<Text>();
                if (presetText != null)
                {
                    presetText.fontSize = HudLayoutConfig.HeroPresetButtonFontSize;
                }

                int capturedPreset = preset;
                presetButton.onClick.AddListener(() => args.OnPresetClick?.Invoke(capturedPreset));
                args.PresetButtons[preset] = presetButton;
            }
        }

        private static void CreateRuneRow(HeroFormationViewBuildArgs args, Transform parent)
        {
            GameObject formationRuneRow = HudUiFactory.CreatePanel("FormationRuneRow", parent, Color.white);
            HudUiFactory.ApplySprite(formationRuneRow.GetComponent<Image>(), HudSpriteKind.ParchmentPanel, new Color(0.52f, 0.64f, 0.78f, 1f));
            HorizontalLayoutGroup formationRuneLayout = formationRuneRow.AddComponent<HorizontalLayoutGroup>();
            formationRuneLayout.padding = new RectOffset(10, 10, 7, 7);
            formationRuneLayout.spacing = 8;
            formationRuneLayout.childControlWidth = true;
            formationRuneLayout.childControlHeight = true;
            formationRuneLayout.childForceExpandWidth = true;
            formationRuneLayout.childForceExpandHeight = true;
            HudUiFactory.AddLayoutElement(formationRuneRow, -1, HudLayoutConfig.HeroFormationRuneRowHeight);

            Text formationRuneTitle = HudUiFactory.CreateText("FormationRuneTitle", formationRuneRow.transform, 18, FontStyle.Bold, TextAnchor.MiddleCenter);
            HudUiFactory.AddLayoutElement(formationRuneTitle.gameObject, 54, -1);
            formationRuneTitle.text = "룬";

            for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
            {
                int capturedSlot = slot;
                Button runeSlotButton = HudUiFactory.CreateButton(string.Empty, formationRuneRow.transform, HudButtonStyle.RuneSlot);
                runeSlotButton.onClick.AddListener(() => args.OnRuneSlotClick?.Invoke(capturedSlot));
                args.RuneSlotButtons[capturedSlot] = runeSlotButton;
                Text runeSlotText = runeSlotButton.GetComponentInChildren<Text>();
                HudUiFactory.ConfigureBestFitText(runeSlotText, 12, 16, 0.86f);

                args.RuneSlotTexts[capturedSlot] = runeSlotText;

                Button removeButton = HudUiFactory.CreateCornerActionButton("-", runeSlotButton.transform, HudButtonStyle.Danger);
                removeButton.onClick.AddListener(() => args.OnRuneSlotRemove?.Invoke(capturedSlot));
                removeButton.gameObject.SetActive(false);
                args.RuneSlotRemoveButtons[capturedSlot] = removeButton;
            }
        }

        private static void CreateRoster(HeroFormationViewBuildArgs args, HeroFormationViewRefs refs, Transform parent)
        {
            IReadOnlyList<HeroDefinition> rosterHeroes = args.RosterHeroes ?? Array.Empty<HeroDefinition>();
            GameObject rosterScroll = HudUiFactory.CreatePanel("HeroRosterScroll", parent, Color.white);
            HudUiFactory.ApplyNinePatchPanel(rosterScroll, HudSpriteKind.WoodPanel, new Color(0.70f, 0.68f, 0.58f, 1f));
            LayoutElement rosterScrollLayout = HudUiFactory.AddLayoutElement(rosterScroll, -1, HudLayoutConfig.HeroRosterMinHeight);
            rosterScrollLayout.flexibleHeight = 1f;

            ScrollRect rosterScrollRect = rosterScroll.AddComponent<ScrollRect>();
            rosterScrollRect.horizontal = false;
            rosterScrollRect.vertical = true;
            rosterScrollRect.movementType = ScrollRect.MovementType.Clamped;
            rosterScrollRect.inertia = false;
            rosterScrollRect.scrollSensitivity = 42f;

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
                Text cardText = button.GetComponentInChildren<Text>();
                HudUiFactory.ConfigureBestFitText(cardText, 12, HudLayoutConfig.HeroRosterCardFontSize, 0.84f);

                args.HeroButtonTexts[hero.Id] = cardText;

                GameObject deployedOverlay = HudUiFactory.CreatePanel(hero.Id + "DeployedOverlay", button.transform, new Color(0f, 0f, 0f, 0.62f));
                HudUiFactory.StretchToParent(deployedOverlay);
                Image overlayImage = deployedOverlay.GetComponent<Image>();
                if (overlayImage != null)
                {
                    overlayImage.raycastTarget = false;
                }

                Text deployedText = HudUiFactory.CreateText(hero.Id + "DeployedOverlayText", deployedOverlay.transform, 20, FontStyle.Bold, TextAnchor.MiddleCenter);
                deployedText.color = new Color(1f, 0.92f, 0.42f, 1f);
                deployedText.text = "배치됨";
                deployedText.raycastTarget = false;
                HudUiFactory.StretchToParent(deployedText.gameObject);
                deployedOverlay.SetActive(false);
                args.HeroRosterDeployedOverlays[hero.Id] = deployedOverlay;

                Button actionButton = HudUiFactory.CreateCornerActionButton("+", button.transform, HudButtonStyle.ActionAdd);
                actionButton.onClick.AddListener(() => args.OnHeroRosterActionClick?.Invoke(heroId));
                args.HeroRosterActionButtons[hero.Id] = actionButton;
                args.HeroNotificationDots[hero.Id] = HudUiFactory.CreateNotificationDot(button.transform, 30f, new Vector2(-13f, -13f));
            }
        }
    }
}
