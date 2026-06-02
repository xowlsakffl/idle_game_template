using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero
{
    public sealed class HeroPanelViewRefs
    {
        public Text PlaceholderText;
    }

    public sealed class HeroPanelViewBuildFooterArgs
    {
        public Transform Parent;
        public Action<HeroPageTab> OnTabClick;
        public Dictionary<HeroPageTab, Button> TabButtons;
    }

    public static class HeroPanelView
    {
        public static void BuildHeader(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(
                HudLayoutConfig.HeroHeaderHorizontalPadding,
                HudLayoutConfig.HeroHeaderHorizontalPadding,
                HudLayoutConfig.HeroHeaderVerticalPadding,
                HudLayoutConfig.HeroHeaderVerticalPadding);
            layout.spacing = HudLayoutConfig.HeroHeaderSpacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Text heroTitle = HudUiFactory.CreateText("HeroGrowthTitle", parent, HudLayoutConfig.HeroTitleFontSize, FontStyle.Bold, TextAnchor.MiddleLeft);
            heroTitle.text = "영웅";
            HudUiFactory.AddLayoutElement(heroTitle.gameObject, -1, HudLayoutConfig.HeroTitleHeight);
        }

        public static HeroPanelViewRefs BuildFooter(HeroPanelViewBuildFooterArgs args)
        {
            if (args == null || args.Parent == null)
            {
                return new HeroPanelViewRefs();
            }

            HeroPanelViewRefs refs = new HeroPanelViewRefs();
            refs.PlaceholderText = HudUiFactory.CreateText("HeroPagePlaceholder", args.Parent, 28, FontStyle.Bold, TextAnchor.MiddleCenter);
            refs.PlaceholderText.text = "준비 중";
            LayoutElement placeholderLayout = HudUiFactory.AddLayoutElement(refs.PlaceholderText.gameObject, -1, 0);
            placeholderLayout.flexibleHeight = 1f;
            refs.PlaceholderText.gameObject.SetActive(false);

            GameObject tabs = new GameObject("HeroPageTabs", typeof(RectTransform));
            tabs.transform.SetParent(args.Parent, false);
            HudUiFactory.AddLayoutElement(tabs, -1, HudLayoutConfig.HeroPageTabsHeight);

            CreateTabButton(args, tabs.transform, HeroPageTab.Formation, "편성", 0);
            CreateTabButton(args, tabs.transform, HeroPageTab.Trait, "특성", 1);
            CreateTabButton(args, tabs.transform, HeroPageTab.Statue, "토템", 2);
            CreateTabButton(args, tabs.transform, HeroPageTab.Seal, "룬", 3);
            return refs;
        }

        private static void CreateTabButton(HeroPanelViewBuildFooterArgs args, Transform parent, HeroPageTab tab, string label, int index)
        {
            Button button = HudUiFactory.CreateButton(label, parent, HudButtonStyle.HeroSubTab);
            HudUiFactory.ApplySpriteButtonState(
                button,
                HudSpriteKind.SmallBlueSquareButton,
                HudSpriteKind.SmallBlueSquareButtonPressed,
                false);
            HudUiFactory.MoveButtonText(button, new Vector2(0f, 2f));
            ConfigureTabButtonRect(button, index, 4, HudLayoutConfig.HeroPageTabSpacing);
            button.onClick.AddListener(() => args.OnTabClick?.Invoke(tab));
            args.TabButtons[tab] = button;
        }

        private static void ConfigureTabButtonRect(Button button, int index, int count, float spacing)
        {
            if (button == null || count <= 0)
            {
                return;
            }

            float minX = Mathf.Clamp01((float)index / count);
            float maxX = Mathf.Clamp01((float)(index + 1) / count);
            float halfSpacing = Mathf.Max(0f, spacing) * 0.5f;

            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(minX, 0f);
            rect.anchorMax = new Vector2(maxX, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(index == 0 ? 0f : halfSpacing, 0f);
            rect.offsetMax = new Vector2(index == count - 1 ? 0f : -halfSpacing, 0f);
        }
    }
}
