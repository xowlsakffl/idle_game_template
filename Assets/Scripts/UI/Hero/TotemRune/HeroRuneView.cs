using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.TotemRune
{
    public sealed class HeroRuneViewRefs
    {
        public GameObject Content;
        public Text SummaryText;
        public Text DetailText;
        public Button EquipButton;
        public Button LevelUpButton;
    }

    public sealed class HeroRuneViewBuildArgs
    {
        public Transform Parent;
        public IReadOnlyList<RuneDefinition> Runes;
        public Action<string> OnSelectRune;
        public Action<string> OnRuneAction;
        public Action OnEquipSelected;
        public Action OnLevelUp;
        public Func<bool> CanLevelUp;
        public Dictionary<string, Button> RuneButtons;
        public Dictionary<string, Text> RuneButtonTexts;
        public Dictionary<string, Button> RuneActionButtons;
    }

    public static class HeroRuneView
    {
        public static HeroRuneViewRefs Build(HeroRuneViewBuildArgs args)
        {
            if (args == null || args.Parent == null)
            {
                return new HeroRuneViewRefs();
            }

            HeroRuneViewRefs refs = new HeroRuneViewRefs();
            refs.Content = HudUiFactory.CreatePanel("HeroRuneContent", args.Parent, new Color(0.23f, 0.30f, 0.44f, 1f));
            VerticalLayoutGroup layout = refs.Content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 12, 12);
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            LayoutElement contentLayout = HudUiFactory.AddLayoutElement(refs.Content, -1, 594);
            contentLayout.flexibleHeight = 1f;

            refs.SummaryText = HudUiFactory.CreateText("HeroRuneSummary", refs.Content.transform, 23, FontStyle.Bold, TextAnchor.MiddleLeft);
            HudUiFactory.AddLayoutElement(refs.SummaryText.gameObject, -1, 42);

            CreateRuneGrid(args, refs.Content.transform);
            CreateDetailPanel(refs);
            CreateActionRow(args, refs);

            refs.Content.SetActive(false);
            return refs;
        }

        private static void CreateRuneGrid(HeroRuneViewBuildArgs args, Transform parent)
        {
            GameObject runeGridPanel = HudUiFactory.CreatePanel("HeroRuneGridPanel", parent, new Color(0.28f, 0.36f, 0.52f, 1f));
            GridLayoutGroup runeGrid = runeGridPanel.AddComponent<GridLayoutGroup>();
            runeGrid.padding = new RectOffset(10, 10, 10, 10);
            runeGrid.spacing = new Vector2(8f, 8f);
            runeGrid.cellSize = new Vector2(128f, 90f);
            runeGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            runeGrid.constraintCount = 5;
            HudUiFactory.AddLayoutElement(runeGridPanel, -1, 310);

            if (args.Runes == null)
            {
                return;
            }

            foreach (RuneDefinition rune in args.Runes)
            {
                Button button = HudUiFactory.CreateButton(string.Empty, runeGridPanel.transform, 15, TotemRuneUiText.GetRuneColor(rune));
                string capturedId = rune.Id;
                button.onClick.AddListener(() => args.OnSelectRune?.Invoke(capturedId));
                args.RuneButtons[rune.Id] = button;
                args.RuneButtonTexts[rune.Id] = button.GetComponentInChildren<Text>();

                Text buttonText = args.RuneButtonTexts[rune.Id];
                HudUiFactory.ConfigureBestFitText(buttonText, 10, 15, 0.86f);

                Button actionButton = HudUiFactory.CreateCornerActionButton("+", button.transform, HudButtonStyle.ActionAdd, 34f, 4f);
                actionButton.onClick.AddListener(() => args.OnRuneAction?.Invoke(capturedId));
                args.RuneActionButtons[rune.Id] = actionButton;
            }
        }

        private static void CreateDetailPanel(HeroRuneViewRefs refs)
        {
            GameObject detailPanel = HudUiFactory.CreatePanel("HeroRuneDetailPanel", refs.Content.transform, new Color(0.20f, 0.26f, 0.39f, 1f));
            HudUiFactory.AddLayoutElement(detailPanel, -1, 116);
            refs.DetailText = HudUiFactory.CreateText("HeroRuneDetailText", detailPanel.transform, 21, FontStyle.Bold, TextAnchor.MiddleLeft);
            RectTransform detailRect = refs.DetailText.GetComponent<RectTransform>();
            detailRect.anchorMin = Vector2.zero;
            detailRect.anchorMax = Vector2.one;
            detailRect.offsetMin = new Vector2(18f, 10f);
            detailRect.offsetMax = new Vector2(-18f, -10f);
            refs.DetailText.resizeTextForBestFit = true;
            refs.DetailText.resizeTextMinSize = 14;
            refs.DetailText.resizeTextMaxSize = 21;
            refs.DetailText.lineSpacing = 0.92f;
        }

        private static void CreateActionRow(HeroRuneViewBuildArgs args, HeroRuneViewRefs refs)
        {
            GameObject actionRow = new GameObject("HeroRuneActions", typeof(RectTransform));
            actionRow.transform.SetParent(refs.Content.transform, false);
            HorizontalLayoutGroup actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
            actionLayout.spacing = 14;
            actionLayout.childControlWidth = true;
            actionLayout.childControlHeight = true;
            actionLayout.childForceExpandWidth = true;
            actionLayout.childForceExpandHeight = true;
            HudUiFactory.AddLayoutElement(actionRow, -1, 68);

            refs.EquipButton = HudUiFactory.CreateButton("장착", actionRow.transform, 25, new Color(0.54f, 0.76f, 0.96f, 1f));
            refs.EquipButton.onClick.AddListener(() => args.OnEquipSelected?.Invoke());

            refs.LevelUpButton = HudUiFactory.CreateButton("강화", actionRow.transform, 25, new Color(0.54f, 0.78f, 0.22f, 1f));
            HudUiFactory.ConfigureHoldRepeat(refs.LevelUpButton, args.OnLevelUp, args.CanLevelUp);
        }
    }
}
