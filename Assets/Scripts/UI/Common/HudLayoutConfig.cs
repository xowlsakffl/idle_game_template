using UnityEngine;

namespace IdleGame.UI.Common
{
    public static class HudLayoutConfig
    {
        private const string LayoutSettingsResourcePath = "HudLayoutSettings";

        private static HudLayoutSettings settings;

        private static HudLayoutSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = Resources.Load<HudLayoutSettings>(LayoutSettingsResourcePath);
                }

                if (settings == null)
                {
                    settings = ScriptableObject.CreateInstance<HudLayoutSettings>();
                }

                return settings;
            }
        }

        public static float HeaderHeight => Settings.HeaderHeight;
        public static float BodyPanelHeight => Settings.BodyPanelHeight;
        public static float GrowthBattlePanelHeight => Settings.GrowthBattlePanelHeight;
        public static float GrowthContentPanelHeight => BodyPanelHeight - GrowthBattlePanelHeight;
        public static float BottomNavHeight => Settings.BottomNavHeight;
        public static int BottomNavFontSize => Settings.BottomNavFontSize;

        public static int HeroHeaderHorizontalPadding => Settings.HeroHeaderHorizontalPadding;
        public static int HeroHeaderVerticalPadding => Settings.HeroHeaderVerticalPadding;
        public static float HeroHeaderSpacing => Settings.HeroHeaderSpacing;
        public static float HeroTitleHeight => Settings.HeroTitleHeight;
        public static int HeroTitleFontSize => Settings.HeroTitleFontSize;
        public static float HeroFormationSummaryHeight => Settings.HeroFormationSummaryHeight;
        public static int HeroFormationSummaryFontSize => Settings.HeroFormationSummaryFontSize;
        public static float HeroFormationAreaHeight => Settings.HeroFormationAreaHeight;
        public static float HeroFormationRuneRowHeight => Settings.HeroFormationRuneRowHeight;
        public static float HeroFormationOwnedEffectHeight => Settings.HeroFormationOwnedEffectHeight;
        public static float HeroFormationActionRowHeight => Settings.HeroFormationActionRowHeight;
        public static int HeroFormationActionButtonFontSize => Settings.HeroFormationActionButtonFontSize;
        public static float HeroPageTabsHeight => Settings.HeroPageTabsHeight;
        public static float HeroPageTabButtonWidth => Settings.HeroPageTabButtonWidth;
        public static float HeroPageTabSpacing => Settings.HeroPageTabSpacing;
        public static int HeroPageTabButtonFontSize => Settings.HeroPageTabButtonFontSize;
        public static float HeroRosterMinHeight => Settings.HeroRosterMinHeight;

        public static float HeroPresetColumnWidth => Settings.HeroPresetColumnWidth;
        public static float HeroPresetTitleHeight => Settings.HeroPresetTitleHeight;
        public static float HeroPresetButtonHeight => Settings.HeroPresetButtonHeight;
        public static int HeroPresetTitleFontSize => Settings.HeroPresetTitleFontSize;
        public static int HeroPresetButtonFontSize => Settings.HeroPresetButtonFontSize;

        public static int HeroRosterColumns => Settings.HeroRosterColumns;
        public static int HeroRosterCardFontSize => Settings.HeroRosterCardFontSize;

        public static Vector2 HeroFormationSlotCellSize => Settings.HeroFormationSlotCellSize;
        public static Vector2 HeroFormationSlotSpacing => Settings.HeroFormationSlotSpacing;
        public static Vector2 HeroRosterCardSize => Settings.HeroRosterCardSize;
        public static Vector2 HeroRosterCardSpacing => Settings.HeroRosterCardSpacing;
    }
}
