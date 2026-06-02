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

        public const float HeroTitleHeight = 30f;
        public const int HeroTitleFontSize = 24;
        public const float HeroFormationSummaryHeight = 28f;
        public const int HeroFormationSummaryFontSize = 21;
        public const float HeroFormationAreaHeight = 252f;
        public const float HeroFormationRuneRowHeight = 58f;
        public const float HeroFormationOwnedEffectHeight = 28f;
        public const float HeroFormationActionRowHeight = 42f;
        public const float HeroPageTabsHeight = 34f;
        public const float HeroPageTabButtonWidth = 78f;
        public const int HeroPageTabButtonFontSize = 16;
        public const float HeroRosterMinHeight = 320f;

        public const float HeroPresetColumnWidth = 72f;
        public const float HeroPresetTitleHeight = 24f;
        public const float HeroPresetButtonHeight = 28f;
        public const int HeroPresetTitleFontSize = 16;
        public const int HeroPresetButtonFontSize = 16;

        public const int HeroRosterColumns = 6;
        public const int HeroRosterCardFontSize = 19;

        public static readonly Vector2 HeroFormationSlotCellSize = new Vector2(150f, 112f);
        public static readonly Vector2 HeroFormationSlotSpacing = new Vector2(8f, 8f);
        public static readonly Vector2 HeroRosterCardSize = new Vector2(112f, 126f);
        public static readonly Vector2 HeroRosterCardSpacing = new Vector2(6f, 8f);
    }
}
