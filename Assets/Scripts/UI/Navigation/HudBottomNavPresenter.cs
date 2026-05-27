using System.Collections.Generic;
using UnityEngine.UI;

namespace IdleGame.UI.Navigation
{
    public sealed class HudBottomNavPresenterArgs
    {
        public Dictionary<HudTab, Button> TabButtons;
        public Dictionary<HudTab, string> TabButtonLabels;
        public HudTab ActiveTab;
        public bool ContentPanelOpen;
        public bool HeroDetailPanelOpen;
    }

    public static class HudBottomNavPresenter
    {
        public static void Refresh(HudBottomNavPresenterArgs args)
        {
            if (args == null)
            {
                return;
            }

            BottomNavView.Refresh(new BottomNavViewRefreshArgs<HudTab>
            {
                TabButtons = args.TabButtons,
                TabButtonLabels = args.TabButtonLabels,
                ActiveTab = args.ActiveTab,
                GrowthTab = HudTab.Growth,
                HeroTab = HudTab.Hero,
                ContentPanelOpen = args.ContentPanelOpen,
                HeroDetailPanelOpen = args.HeroDetailPanelOpen,
                GetCloseLabel = GetCloseLabel
            });
        }

        private static string GetCloseLabel(HudTab tab)
        {
            switch (tab)
            {
                case HudTab.Growth:
                    return "X\n성장";
                case HudTab.Hero:
                    return "X\n영웅";
                default:
                    return "X";
            }
        }
    }
}
