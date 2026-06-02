using UnityEngine;

namespace IdleGame.UI.Common
{
    public readonly struct HudButtonVisualStyle
    {
        public HudButtonVisualStyle(int fontSize, Color color)
        {
            FontSize = fontSize;
            Color = color;
        }

        public int FontSize { get; }
        public Color Color { get; }
    }

    public static class HudButtonStyle
    {
        public static readonly HudButtonVisualStyle Primary = new HudButtonVisualStyle(20, new Color(0.72f, 0.56f, 0.15f, 1f));
        public static readonly HudButtonVisualStyle Secondary = new HudButtonVisualStyle(20, new Color(0.34f, 0.35f, 0.37f, 1f));
        public static readonly HudButtonVisualStyle Danger = new HudButtonVisualStyle(20, new Color(0.58f, 0.12f, 0.12f, 1f));
        public static readonly HudButtonVisualStyle Tab = new HudButtonVisualStyle(20, new Color(0.18f, 0.24f, 0.38f, 1f));
        public static readonly HudButtonVisualStyle TabSelected = new HudButtonVisualStyle(20, new Color(0.42f, 0.54f, 0.82f, 1f));
        public static HudButtonVisualStyle HeroSubTab => new HudButtonVisualStyle(HudLayoutConfig.HeroPageTabButtonFontSize, new Color(0.18f, 0.24f, 0.38f, 1f));
        public static readonly HudButtonVisualStyle SmallPreset = new HudButtonVisualStyle(18, new Color(0.21f, 0.29f, 0.45f, 1f));
        public static readonly HudButtonVisualStyle SmallPresetSelected = new HudButtonVisualStyle(18, new Color(0.50f, 0.64f, 0.96f, 1f));
        public static readonly HudButtonVisualStyle Slot = new HudButtonVisualStyle(18, new Color(0.18f, 0.22f, 0.31f, 1f));
        public static readonly HudButtonVisualStyle RuneSlot = new HudButtonVisualStyle(16, new Color(0.17f, 0.21f, 0.31f, 1f));
        public static readonly HudButtonVisualStyle LockedSlot = new HudButtonVisualStyle(16, new Color(0.18f, 0.20f, 0.26f, 1f));
        public static readonly HudButtonVisualStyle ActionAdd = new HudButtonVisualStyle(20, new Color(0.88f, 0.72f, 0.20f, 1f));
        public static readonly HudButtonVisualStyle Disabled = new HudButtonVisualStyle(20, new Color(0.24f, 0.25f, 0.28f, 1f));
    }
}
