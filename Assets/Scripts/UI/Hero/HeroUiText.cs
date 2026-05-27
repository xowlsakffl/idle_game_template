using UnityEngine;
using IdleGame.Data;

namespace IdleGame.UI.Hero
{
    public static class HeroUiText
    {
        public static string GetRarityBadge(HeroRarity rarity)
        {
            switch (rarity)
            {
                case HeroRarity.Common:
                    return "C";
                case HeroRarity.Uncommon:
                    return "UC";
                case HeroRarity.Rare:
                    return "R";
                case HeroRarity.Epic:
                    return "E";
                case HeroRarity.Legendary:
                    return "L";
                case HeroRarity.Mythic:
                    return "M";
                default:
                    return "?";
            }
        }

        public static string GetRarityLabel(HeroRarity rarity)
        {
            switch (rarity)
            {
                case HeroRarity.Common:
                    return "커먼";
                case HeroRarity.Uncommon:
                    return "언커먼";
                case HeroRarity.Rare:
                    return "레어";
                case HeroRarity.Epic:
                    return "에픽";
                case HeroRarity.Legendary:
                    return "레전더리";
                case HeroRarity.Mythic:
                    return "신화";
                default:
                    return "미정";
            }
        }

        public static string GetTraitBadge(HeroTrait trait)
        {
            switch (trait)
            {
                case HeroTrait.Melee:
                    return "근";
                case HeroTrait.Ranged:
                    return "원";
                case HeroTrait.Support:
                    return "지";
                case HeroTrait.Defense:
                    return "방";
                default:
                    return "?";
            }
        }

        public static string GetTraitLabel(HeroTrait trait)
        {
            switch (trait)
            {
                case HeroTrait.Melee:
                    return "근접형";
                case HeroTrait.Ranged:
                    return "원거리형";
                case HeroTrait.Support:
                    return "지원형";
                case HeroTrait.Defense:
                    return "방어형";
                default:
                    return "미정";
            }
        }

        public static Color GetTranscendGradeColor(HeroTranscendGrade grade)
        {
            switch (grade)
            {
                case HeroTranscendGrade.F:
                    return new Color(0.43f, 0.49f, 0.58f, 1f);
                case HeroTranscendGrade.E:
                    return new Color(0.33f, 0.55f, 0.70f, 1f);
                case HeroTranscendGrade.D:
                    return new Color(0.26f, 0.60f, 0.46f, 1f);
                case HeroTranscendGrade.C:
                    return new Color(0.42f, 0.64f, 0.24f, 1f);
                case HeroTranscendGrade.B:
                    return new Color(0.66f, 0.58f, 0.22f, 1f);
                case HeroTranscendGrade.A:
                    return new Color(0.84f, 0.45f, 0.18f, 1f);
                case HeroTranscendGrade.S:
                    return new Color(0.78f, 0.28f, 0.86f, 1f);
                case HeroTranscendGrade.SS:
                    return new Color(0.33f, 0.72f, 1f, 1f);
                default:
                    return Color.white;
            }
        }

        public static string GetTranscendGradeHex(HeroTranscendGrade grade)
        {
            switch (grade)
            {
                case HeroTranscendGrade.F:
                    return "#A9B2C4";
                case HeroTranscendGrade.E:
                    return "#86C8FF";
                case HeroTranscendGrade.D:
                    return "#65D29B";
                case HeroTranscendGrade.C:
                    return "#9BDA5A";
                case HeroTranscendGrade.B:
                    return "#FFD65A";
                case HeroTranscendGrade.A:
                    return "#FF9C4A";
                case HeroTranscendGrade.S:
                    return "#E66BFF";
                case HeroTranscendGrade.SS:
                    return "#6DD7FF";
                default:
                    return "#FFFFFF";
            }
        }

        public static Color GetRarityColor(HeroRarity rarity)
        {
            switch (rarity)
            {
                case HeroRarity.Common:
                    return new Color(0.36f, 0.38f, 0.40f, 1f);
                case HeroRarity.Uncommon:
                    return new Color(0.18f, 0.36f, 0.25f, 1f);
                case HeroRarity.Rare:
                    return new Color(0.16f, 0.32f, 0.58f, 1f);
                case HeroRarity.Epic:
                    return new Color(0.44f, 0.20f, 0.62f, 1f);
                case HeroRarity.Legendary:
                    return new Color(0.76f, 0.47f, 0.12f, 1f);
                case HeroRarity.Mythic:
                    return new Color(0.64f, 0.16f, 0.18f, 1f);
                default:
                    return new Color(0.16f, 0.24f, 0.34f, 1f);
            }
        }
    }
}
