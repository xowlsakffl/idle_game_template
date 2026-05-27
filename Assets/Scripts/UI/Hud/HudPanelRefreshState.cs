using IdleGame.UI.Common;
using IdleGame.UI.Navigation;

namespace IdleGame.UI.Hud
{
    public sealed class HudPanelRefreshStateBuildArgs
    {
        public HudDirtyFlags DirtyFlags;
        public HudTab ActiveTab;
        public bool ContentPanelOpen;
        public bool HeroDetailPanelOpen;
        public HudTab LastRenderedActiveTab;
        public bool LastRenderedContentPanelOpen;
        public bool LastRenderedHeroDetailPanelOpen;
    }

    public sealed class HudPanelRefreshState
    {
        public bool GrowthPanelOpen;
        public bool HeroPanelOpen;
        public bool FortressPanelOpen;
        public bool FacilityPanelOpen;
        public bool StagePanelOpen;
        public bool SummonPanelOpen;
        public bool ShopPanelOpen;
        public bool SupportPanelOpen;
        public bool DebugPanelOpen;
        public bool RefreshGrowthPanel;
        public bool RefreshHeroPanel;
        public bool RefreshFortressPanel;
        public bool RefreshFacilityPanel;
        public bool RefreshStagePanel;
        public bool RefreshSummonPanel;
        public bool RefreshSupportPanel;
        public bool RefreshDebugPanel;
        public bool RefreshHeroDetailPanel;
        public float BattlePanelHeight;
        public float ContentPanelHeight;

        public static HudPanelRefreshState Build(HudPanelRefreshStateBuildArgs args)
        {
            var state = new HudPanelRefreshState();
            if (args == null)
            {
                return state;
            }

            state.GrowthPanelOpen = args.ContentPanelOpen && args.ActiveTab == HudTab.Growth;
            state.HeroPanelOpen = args.ContentPanelOpen && args.ActiveTab == HudTab.Hero;
            state.FortressPanelOpen = args.ContentPanelOpen && args.ActiveTab == HudTab.Fortress;
            state.FacilityPanelOpen = args.ContentPanelOpen && args.ActiveTab == HudTab.Facility;
            state.StagePanelOpen = args.ContentPanelOpen && args.ActiveTab == HudTab.Stage;
            state.SummonPanelOpen = args.ContentPanelOpen && args.ActiveTab == HudTab.Summon;
            state.ShopPanelOpen = args.ContentPanelOpen && args.ActiveTab == HudTab.Shop;
            state.SupportPanelOpen = args.ContentPanelOpen && args.ActiveTab == HudTab.Support;
            state.DebugPanelOpen = args.ContentPanelOpen && args.ActiveTab == HudTab.Debug;

            bool activeTabJustOpened = args.ContentPanelOpen
                && (!args.LastRenderedContentPanelOpen || args.LastRenderedActiveTab != args.ActiveTab);
            bool heroDetailJustOpened = args.HeroDetailPanelOpen && !args.LastRenderedHeroDetailPanelOpen;

            state.RefreshGrowthPanel = state.GrowthPanelOpen && ShouldRefreshUiSection(args.DirtyFlags, HudDirtyFlags.Growth, activeTabJustOpened);
            state.RefreshHeroPanel = state.HeroPanelOpen && ShouldRefreshUiSection(args.DirtyFlags, HudDirtyFlags.Hero, activeTabJustOpened);
            state.RefreshFortressPanel = state.FortressPanelOpen && ShouldRefreshUiSection(args.DirtyFlags, HudDirtyFlags.Fortress, activeTabJustOpened);
            state.RefreshFacilityPanel = state.FacilityPanelOpen && ShouldRefreshUiSection(args.DirtyFlags, HudDirtyFlags.Facility, activeTabJustOpened);
            state.RefreshStagePanel = state.StagePanelOpen && ShouldRefreshUiSection(args.DirtyFlags, HudDirtyFlags.Stage, activeTabJustOpened);
            state.RefreshSummonPanel = state.SummonPanelOpen && ShouldRefreshUiSection(args.DirtyFlags, HudDirtyFlags.Summon, activeTabJustOpened);
            state.RefreshSupportPanel = state.SupportPanelOpen && ShouldRefreshUiSection(args.DirtyFlags, HudDirtyFlags.Support, activeTabJustOpened);
            state.RefreshDebugPanel = state.DebugPanelOpen && ShouldRefreshUiSection(args.DirtyFlags, HudDirtyFlags.Debug, activeTabJustOpened);
            state.RefreshHeroDetailPanel = args.HeroDetailPanelOpen && ShouldRefreshUiSection(args.DirtyFlags, HudDirtyFlags.HeroDetail, heroDetailJustOpened);

            state.BattlePanelHeight = GetBattlePanelHeight(args.ActiveTab, args.ContentPanelOpen);
            state.ContentPanelHeight = GetContentPanelHeight(args.ActiveTab, args.ContentPanelOpen);
            return state;
        }

        private static bool ShouldRefreshUiSection(HudDirtyFlags currentDirty, HudDirtyFlags section, bool justOpened)
        {
            if (justOpened || currentDirty == HudDirtyFlags.None || currentDirty == HudDirtyFlags.All)
            {
                return true;
            }

            return (currentDirty & section) != 0;
        }

        private static float GetBattlePanelHeight(HudTab activeTab, bool contentPanelOpen)
        {
            if (!contentPanelOpen)
            {
                return HudLayoutConfig.BodyPanelHeight;
            }

            return activeTab == HudTab.Growth ? HudLayoutConfig.GrowthBattlePanelHeight : 0f;
        }

        private static float GetContentPanelHeight(HudTab activeTab, bool contentPanelOpen)
        {
            if (!contentPanelOpen)
            {
                return 0f;
            }

            return activeTab == HudTab.Growth ? HudLayoutConfig.GrowthContentPanelHeight : HudLayoutConfig.BodyPanelHeight;
        }
    }
}
