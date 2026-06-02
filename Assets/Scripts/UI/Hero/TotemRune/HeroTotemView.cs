using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.TotemRune
{
    public sealed class HeroTotemViewRefs
    {
        public GameObject Content;
        public Text SummaryText;
        public Text DetailText;
        public Button LevelUpButton;
    }

    public sealed class HeroTotemViewBuildArgs
    {
        public Transform Parent;
        public IReadOnlyList<TotemDefinition> Totems;
        public Action<string> OnSelectTotem;
        public Action OnLevelUp;
        public Func<bool> CanLevelUp;
        public Dictionary<string, Button> TotemButtons;
        public Dictionary<string, Text> TotemButtonTexts;
    }

    public static class HeroTotemView
    {
        public static HeroTotemViewRefs Build(HeroTotemViewBuildArgs args)
        {
            if (args == null || args.Parent == null)
            {
                return new HeroTotemViewRefs();
            }

            HeroTotemViewRefs refs = new HeroTotemViewRefs();
            refs.Content = HudUiFactory.CreatePanel("HeroTotemContent", args.Parent, new Color(0.24f, 0.31f, 0.45f, 1f));
            VerticalLayoutGroup layout = refs.Content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 12, 12);
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            LayoutElement contentLayout = HudUiFactory.AddLayoutElement(refs.Content, -1, 594);
            contentLayout.flexibleHeight = 1f;

            refs.SummaryText = HudUiFactory.CreateText("HeroTotemSummary", refs.Content.transform, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
            HudUiFactory.AddLayoutElement(refs.SummaryText.gameObject, -1, 42);

            GameObject circlePanel = CreateCirclePanel(args, refs);
            CreateTotemNodes(args, circlePanel.transform);
            CreateActionRow(args, refs);

            refs.Content.SetActive(false);
            return refs;
        }

        private static GameObject CreateCirclePanel(HeroTotemViewBuildArgs args, HeroTotemViewRefs refs)
        {
            GameObject circlePanel = HudUiFactory.CreatePanel("HeroTotemCirclePanel", refs.Content.transform, new Color(0.30f, 0.39f, 0.56f, 1f));
            HudUiFactory.AddLayoutElement(circlePanel, -1, 420);

            GameObject centerPanel = HudUiFactory.CreatePanel("HeroTotemCenterEffect", circlePanel.transform, new Color(0.18f, 0.24f, 0.36f, 0.94f));
            RectTransform centerRect = centerPanel.GetComponent<RectTransform>();
            centerRect.anchorMin = new Vector2(0.5f, 0.5f);
            centerRect.anchorMax = new Vector2(0.5f, 0.5f);
            centerRect.pivot = new Vector2(0.5f, 0.5f);
            centerRect.sizeDelta = new Vector2(360f, 176f);
            centerRect.anchoredPosition = Vector2.zero;

            refs.DetailText = HudUiFactory.CreateText("HeroTotemDetailText", centerPanel.transform, 20, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform detailTextRect = refs.DetailText.GetComponent<RectTransform>();
            detailTextRect.anchorMin = Vector2.zero;
            detailTextRect.anchorMax = Vector2.one;
            detailTextRect.offsetMin = new Vector2(18f, 12f);
            detailTextRect.offsetMax = new Vector2(-18f, -12f);
            refs.DetailText.resizeTextForBestFit = true;
            refs.DetailText.resizeTextMinSize = 15;
            refs.DetailText.resizeTextMaxSize = 20;
            refs.DetailText.lineSpacing = 0.92f;

            return circlePanel;
        }

        private static void CreateTotemNodes(HeroTotemViewBuildArgs args, Transform parent)
        {
            if (args.Totems == null)
            {
                return;
            }

            foreach (TotemDefinition totem in args.Totems)
            {
                Button button = HudUiFactory.CreateButton(string.Empty, parent, 18, TotemRuneUiText.GetTotemColor(totem));
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                buttonRect.pivot = new Vector2(0.5f, 0.5f);
                buttonRect.sizeDelta = new Vector2(132f, 132f);
                buttonRect.anchoredPosition = GetCirclePosition(totem.Archetype);

                string capturedId = totem.Id;
                button.onClick.AddListener(() => args.OnSelectTotem?.Invoke(capturedId));
                args.TotemButtons[totem.Id] = button;
                args.TotemButtonTexts[totem.Id] = button.GetComponentInChildren<Text>();
                Text buttonText = args.TotemButtonTexts[totem.Id];
                HudUiFactory.ConfigureBestFitText(buttonText, 11, 18, 0.86f);
            }
        }

        private static void CreateActionRow(HeroTotemViewBuildArgs args, HeroTotemViewRefs refs)
        {
            GameObject actionRow = new GameObject("HeroTotemActions", typeof(RectTransform));
            actionRow.transform.SetParent(refs.Content.transform, false);
            HorizontalLayoutGroup actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
            actionLayout.spacing = 14;
            actionLayout.childControlWidth = true;
            actionLayout.childControlHeight = true;
            actionLayout.childForceExpandWidth = true;
            actionLayout.childForceExpandHeight = true;
            HudUiFactory.AddLayoutElement(actionRow, -1, 68);

            refs.LevelUpButton = HudUiFactory.CreateButton("강화", actionRow.transform, 26, new Color(0.54f, 0.78f, 0.22f, 1f));
            HudUiFactory.ConfigureHoldRepeat(refs.LevelUpButton, args.OnLevelUp, args.CanLevelUp);
        }

        private static Vector2 GetCirclePosition(TotemArchetype archetype)
        {
            switch (archetype)
            {
                case TotemArchetype.Command:
                    return new Vector2(0f, 190f);
                case TotemArchetype.Combat:
                    return new Vector2(-250f, 126f);
                case TotemArchetype.Guardian:
                    return new Vector2(250f, 126f);
                case TotemArchetype.Storm:
                    return new Vector2(250f, -126f);
                case TotemArchetype.Support:
                    return new Vector2(0f, -190f);
                case TotemArchetype.Arcane:
                    return new Vector2(-250f, -126f);
                default:
                    return Vector2.zero;
            }
        }
    }
}
