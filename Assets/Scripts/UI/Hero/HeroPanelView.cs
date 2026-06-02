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
            layout.padding = new RectOffset(20, 20, 14, 14);
            layout.spacing = 6;
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
            HorizontalLayoutGroup tabLayout = tabs.AddComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = 6;
            tabLayout.childAlignment = TextAnchor.MiddleCenter;
            tabLayout.childControlWidth = true;
            tabLayout.childControlHeight = true;
            tabLayout.childForceExpandWidth = false;
            tabLayout.childForceExpandHeight = true;
            HudUiFactory.AddLayoutElement(tabs, -1, HudLayoutConfig.HeroPageTabsHeight);

            CreateTabButton(args, tabs.transform, HeroPageTab.Formation, "편성");
            CreateTabButton(args, tabs.transform, HeroPageTab.Trait, "특성");
            CreateTabButton(args, tabs.transform, HeroPageTab.Statue, "토템");
            CreateTabButton(args, tabs.transform, HeroPageTab.Seal, "룬");
            return refs;
        }

        private static void CreateTabButton(HeroPanelViewBuildFooterArgs args, Transform parent, HeroPageTab tab, string label)
        {
            Button button = HudUiFactory.CreateButton(label, parent, HudButtonStyle.HeroSubTab);
            HudUiFactory.AddLayoutElement(button.gameObject, HudLayoutConfig.HeroPageTabButtonWidth, -1);
            button.onClick.AddListener(() => args.OnTabClick?.Invoke(tab));
            args.TabButtons[tab] = button;
        }
    }
}
