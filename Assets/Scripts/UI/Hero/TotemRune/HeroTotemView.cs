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
        public Button EquipButton;
        public Button LevelUpButton;
    }

    public sealed class HeroTotemViewBuildArgs
    {
        public Transform Parent;
        public IReadOnlyList<TotemDefinition> Totems;
        public Action<string> OnSelectTotem;
        public Action<string> OnTotemAction;
        public Action OnEquipSelected;
        public Action OnLevelUp;
        public Func<bool> CanLevelUp;
        public Dictionary<string, Button> TotemButtons;
        public Dictionary<string, Text> TotemButtonTexts;
        public Dictionary<string, Button> TotemActionButtons;
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

            GameObject ring = new GameObject("HeroTotemCircleRing", typeof(RectTransform), typeof(Image));
            ring.transform.SetParent(circlePanel.transform, false);
            Image ringImage = ring.GetComponent<Image>();
            ringImage.sprite = HudSpriteFactory.GetRingSprite();
            ringImage.color = new Color(0.70f, 0.84f, 1f, 0.28f);
            ringImage.raycastTarget = false;
            RectTransform ringRect = ring.GetComponent<RectTransform>();
            ringRect.anchorMin = new Vector2(0.5f, 0.5f);
            ringRect.anchorMax = new Vector2(0.5f, 0.5f);
            ringRect.pivot = new Vector2(0.5f, 0.5f);
            ringRect.sizeDelta = new Vector2(390f, 390f);
            ringRect.anchoredPosition = Vector2.zero;

            GameObject centerPanel = HudUiFactory.CreatePanel("HeroTotemCenterEffect", circlePanel.transform, new Color(0.18f, 0.24f, 0.36f, 0.94f));
            RectTransform centerRect = centerPanel.GetComponent<RectTransform>();
            centerRect.anchorMin = new Vector2(0.5f, 0.5f);
            centerRect.anchorMax = new Vector2(0.5f, 0.5f);
            centerRect.pivot = new Vector2(0.5f, 0.5f);
            centerRect.sizeDelta = new Vector2(360f, 176f);
            centerRect.anchoredPosition = Vector2.zero;

            GameObject centerGlow = new GameObject("HeroTotemCenterGlow", typeof(RectTransform), typeof(Image));
            centerGlow.transform.SetParent(centerPanel.transform, false);
            Image centerGlowImage = centerGlow.GetComponent<Image>();
            centerGlowImage.sprite = HudSpriteFactory.GetCircleSprite();
            centerGlowImage.color = new Color(0.55f, 0.78f, 1f, 0.08f);
            centerGlowImage.raycastTarget = false;
            RectTransform centerGlowRect = centerGlow.GetComponent<RectTransform>();
            centerGlowRect.anchorMin = new Vector2(0.5f, 0.5f);
            centerGlowRect.anchorMax = new Vector2(0.5f, 0.5f);
            centerGlowRect.pivot = new Vector2(0.5f, 0.5f);
            centerGlowRect.sizeDelta = new Vector2(330f, 330f);
            centerGlowRect.anchoredPosition = Vector2.zero;

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
                ConfigureNodeButton(button);

                string capturedId = totem.Id;
                button.onClick.AddListener(() => args.OnSelectTotem?.Invoke(capturedId));
                args.TotemButtons[totem.Id] = button;
                args.TotemButtonTexts[totem.Id] = button.GetComponentInChildren<Text>();
                Text buttonText = args.TotemButtonTexts[totem.Id];
                if (buttonText != null)
                {
                    buttonText.resizeTextForBestFit = true;
                    buttonText.resizeTextMinSize = 11;
                    buttonText.resizeTextMaxSize = 18;
                    buttonText.lineSpacing = 0.86f;
                }

                Button actionButton = CreateCornerActionButton("+", button.transform, new Color(0.88f, 0.72f, 0.20f, 1f));
                actionButton.onClick.AddListener(() => args.OnTotemAction?.Invoke(capturedId));
                actionButton.gameObject.SetActive(false);
                args.TotemActionButtons[totem.Id] = actionButton;
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

            refs.EquipButton = HudUiFactory.CreateButton("장착", actionRow.transform, 26, new Color(0.54f, 0.76f, 0.96f, 1f));
            refs.EquipButton.onClick.AddListener(() => args.OnEquipSelected?.Invoke());
            refs.EquipButton.gameObject.SetActive(false);

            refs.LevelUpButton = HudUiFactory.CreateButton("강화", actionRow.transform, 26, new Color(0.54f, 0.78f, 0.22f, 1f));
            ConfigureHoldRepeat(refs.LevelUpButton, args.OnLevelUp, args.CanLevelUp);
        }

        private static void ConfigureNodeButton(Button button)
        {
            if (button == null)
            {
                return;
            }

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = HudSpriteFactory.GetCircleSprite();
                buttonImage.type = Image.Type.Simple;
            }

            GameObject rim = new GameObject("TotemNodeRim", typeof(RectTransform), typeof(Image));
            rim.transform.SetParent(button.transform, false);
            rim.transform.SetAsFirstSibling();
            Image rimImage = rim.GetComponent<Image>();
            rimImage.sprite = HudSpriteFactory.GetRingSprite();
            rimImage.color = new Color(0.02f, 0.04f, 0.08f, 0.72f);
            rimImage.raycastTarget = false;
            HudUiFactory.StretchToParent(rim);

            GameObject inner = new GameObject("TotemNodeInnerGlow", typeof(RectTransform), typeof(Image));
            inner.transform.SetParent(button.transform, false);
            inner.transform.SetAsFirstSibling();
            Image innerImage = inner.GetComponent<Image>();
            innerImage.sprite = HudSpriteFactory.GetCircleSprite();
            innerImage.color = new Color(1f, 1f, 1f, 0.10f);
            innerImage.raycastTarget = false;
            RectTransform innerRect = inner.GetComponent<RectTransform>();
            innerRect.anchorMin = new Vector2(0.14f, 0.14f);
            innerRect.anchorMax = new Vector2(0.86f, 0.86f);
            innerRect.offsetMin = Vector2.zero;
            innerRect.offsetMax = Vector2.zero;
        }

        private static Button CreateCornerActionButton(string label, Transform parent, Color color)
        {
            Button button = HudUiFactory.CreateButton(label, parent, 20, color);
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(34f, 34f);
            rect.anchoredPosition = new Vector2(-4f, -4f);
            return button;
        }

        private static void ConfigureHoldRepeat(Button button, Action action, Func<bool> canRepeat)
        {
            if (button == null)
            {
                return;
            }

            HoldRepeatButton repeatButton = button.GetComponent<HoldRepeatButton>();
            if (repeatButton == null)
            {
                repeatButton = button.gameObject.AddComponent<HoldRepeatButton>();
            }

            repeatButton.Configure(action, canRepeat);
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
