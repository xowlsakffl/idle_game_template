using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.UI.Common;

namespace IdleGame.UI.Navigation
{
    public sealed class BottomNavItem<TTab>
    {
        public TTab Tab;
        public string Label;
    }

    public sealed class BottomNavViewBuildArgs<TTab>
    {
        public Transform Parent;
        public IReadOnlyList<BottomNavItem<TTab>> Items;
        public Action<TTab> OnTabClick;
        public Dictionary<TTab, Button> TabButtons;
        public Dictionary<TTab, string> TabButtonLabels;
        public Dictionary<TTab, List<GameObject>> TabNotificationDots;
    }

    public sealed class BottomNavViewRefreshArgs<TTab>
    {
        public Dictionary<TTab, Button> TabButtons;
        public Dictionary<TTab, string> TabButtonLabels;
        public TTab ActiveTab;
        public TTab GrowthTab;
        public TTab HeroTab;
        public bool ContentPanelOpen;
        public bool HeroDetailPanelOpen;
        public Func<TTab, string> GetCloseLabel;
    }

    public static class BottomNavView
    {
        public static void Build<TTab>(BottomNavViewBuildArgs<TTab> args)
        {
            if (args == null || args.Parent == null)
            {
                return;
            }

            GameObject panel = HudUiFactory.CreatePanel("BottomNav", args.Parent, new Color(0.08f, 0.11f, 0.17f, 1f));
            HudUiFactory.AddLayoutElement(panel, -1, HudLayoutConfig.BottomNavHeight);

            HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(3, 3, 3, 3);
            layout.spacing = 2;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            if (args.Items == null)
            {
                return;
            }

            for (int i = 0; i < args.Items.Count; i++)
            {
                BottomNavItem<TTab> item = args.Items[i];
                CreateTabButton(args, panel.transform, item.Tab, item.Label);
            }
        }

        public static void Refresh<TTab>(BottomNavViewRefreshArgs<TTab> args)
        {
            if (args == null || args.TabButtons == null)
            {
                return;
            }

            EqualityComparer<TTab> comparer = EqualityComparer<TTab>.Default;
            foreach (KeyValuePair<TTab, Button> pair in args.TabButtons)
            {
                Text text = pair.Value.GetComponentInChildren<Text>(true);
                if (text == null)
                {
                    continue;
                }

                bool activeAndOpen = args.ContentPanelOpen && comparer.Equals(pair.Key, args.ActiveTab);
                bool heroDetailCloseTab = args.HeroDetailPanelOpen && comparer.Equals(pair.Key, args.HeroTab);
                bool selected = activeAndOpen || heroDetailCloseTab;
                string label = args.TabButtonLabels != null && args.TabButtonLabels.TryGetValue(pair.Key, out string savedLabel)
                    ? savedLabel
                    : text.text;

                if (heroDetailCloseTab)
                {
                    text.text = "X\n영웅";
                }
                else if ((comparer.Equals(pair.Key, args.GrowthTab) || comparer.Equals(pair.Key, args.HeroTab)) && activeAndOpen)
                {
                    text.text = args.GetCloseLabel != null ? args.GetCloseLabel(pair.Key) : label;
                }
                else
                {
                    text.text = label;
                }

                text.color = Color.white;
                ApplyMenuButtonSprite(pair.Value, selected);
            }
        }

        private static void CreateTabButton<TTab>(BottomNavViewBuildArgs<TTab> args, Transform parent, TTab tab, string label)
        {
            Button button = HudUiFactory.CreateButton(label, parent, HudLayoutConfig.BottomNavFontSize, Color.white);
            ApplyMenuButtonSprite(button, false);
            HudUiFactory.MoveButtonText(button, new Vector2(0f, 2f));
            Text text = button.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 13;
                text.resizeTextMaxSize = HudLayoutConfig.BottomNavFontSize;
            }

            RegisterNotificationDot(args.TabNotificationDots, tab, HudUiFactory.CreateNotificationDot(button.transform, 38f, new Vector2(-14f, -14f)));
            button.onClick.AddListener(() => args.OnTabClick?.Invoke(tab));
            args.TabButtons[tab] = button;
            args.TabButtonLabels[tab] = label;
        }

        private static void ApplyMenuButtonSprite(Button button, bool selected)
        {
            if (button == null)
            {
                return;
            }

            HudUiFactory.ApplySpriteButtonState(
                button,
                HudSpriteKind.SmallBlueSquareButton,
                HudSpriteKind.SmallBlueSquareButtonPressed,
                selected);
        }

        private static void RegisterNotificationDot<TTab>(Dictionary<TTab, List<GameObject>> dotsByTab, TTab tab, GameObject dot)
        {
            if (dotsByTab == null || dot == null)
            {
                return;
            }

            if (!dotsByTab.TryGetValue(tab, out List<GameObject> dots))
            {
                dots = new List<GameObject>();
                dotsByTab[tab] = dots;
            }

            dots.Add(dot);
            dot.SetActive(false);
        }
    }
}
