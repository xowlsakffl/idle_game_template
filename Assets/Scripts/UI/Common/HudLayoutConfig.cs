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
        public const float HeroFormationSummaryHeight = 30f;
        public const int HeroFormationSummaryFontSize = 20;
        public const float HeroFormationAreaHeight = 286f;
        public const float HeroFormationRuneRowHeight = 72f;
        public const float HeroFormationOwnedEffectHeight = 32f;
        public const float HeroFormationActionRowHeight = 52f;
        public const float HeroPageTabsHeight = 56f;
        public const float HeroRosterMinHeight = 240f;

        public const float HeroPresetColumnWidth = 76f;
        public const float HeroPresetTitleHeight = 26f;
        public const float HeroPresetButtonHeight = 34f;
        public const int HeroPresetTitleFontSize = 17;
        public const int HeroPresetButtonFontSize = 18;

        public const int HeroRosterColumns = 6;
        public const int HeroRosterCardFontSize = 18;

        public static readonly Vector2 HeroFormationSlotCellSize = new Vector2(118f, 104f);
        public static readonly Vector2 HeroFormationSlotSpacing = new Vector2(10f, 12f);
        public static readonly Vector2 HeroRosterCardSize = new Vector2(104f, 118f);
        public static readonly Vector2 HeroRosterCardSpacing = new Vector2(6f, 8f);
    }
}
