using UnityEngine;

namespace IdleGame.UI.Common
{
    public static class HudLayoutConfig
    {
        public const float HeaderHeight = 160f;
        public const float BodyPanelHeight = 1648f;
        public const float GrowthBattlePanelHeight = 870f;
        public const float GrowthContentPanelHeight = BodyPanelHeight - GrowthBattlePanelHeight;
        public const float BottomNavHeight = 112f;
        public const int BottomNavFontSize = 19;

        public const float HeroTitleHeight = 32f;
        public const int HeroTitleFontSize = 26;
        public const float HeroFormationSummaryHeight = 28f;
        public const int HeroFormationSummaryFontSize = 21;
        public const float HeroFormationAreaHeight = 246f;
        public const float HeroFormationRuneRowHeight = 56f;
        public const float HeroFormationOwnedEffectHeight = 28f;
        public const float HeroFormationActionRowHeight = 44f;
        public const float HeroPageTabsHeight = 44f;
        public const float HeroRosterMinHeight = 160f;

        public const float HeroPresetColumnWidth = 96f;
        public const float HeroPresetTitleHeight = 28f;
        public const float HeroPresetButtonHeight = 40f;
        public const int HeroPresetTitleFontSize = 20;
        public const int HeroPresetButtonFontSize = 20;

        public const int HeroRosterColumns = 6;
        public const int HeroRosterCardFontSize = 17;

        public static readonly Vector2 HeroFormationSlotCellSize = new Vector2(148f, 74f);
        public static readonly Vector2 HeroFormationSlotSpacing = new Vector2(10f, 12f);
        public static readonly Vector2 HeroRosterCardSize = new Vector2(160f, 132f);
        public static readonly Vector2 HeroRosterCardSpacing = new Vector2(10f, 10f);
    }
}
