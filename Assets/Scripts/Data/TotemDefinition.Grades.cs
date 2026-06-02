using System;
using UnityEngine;

namespace IdleGame.Data
{
    public sealed partial class TotemDefinition
    {
        public static TotemGrade GetNextGrade(TotemGrade grade)
        {
            return grade >= TotemGrade.Mythic ? TotemGrade.Mythic : (TotemGrade)((int)grade + 1);
        }

        public static string GetGradeLabel(TotemGrade grade)
        {
            switch (grade)
            {
                case TotemGrade.Common:
                    return "커먼";
                case TotemGrade.Uncommon:
                    return "언커먼";
                case TotemGrade.Rare:
                    return "레어";
                case TotemGrade.Epic:
                    return "에픽";
                case TotemGrade.Legendary:
                    return "전설";
                case TotemGrade.Mythic:
                    return "신화";
                default:
                    return grade.ToString();
            }
        }

        private static string GetGradePrefix(TotemGrade grade)
        {
            switch (grade)
            {
                case TotemGrade.Common:
                    return "낡은";
                case TotemGrade.Uncommon:
                    return "정제된";
                case TotemGrade.Rare:
                    return "용맹의";
                case TotemGrade.Epic:
                    return "영웅의";
                case TotemGrade.Legendary:
                    return "고대";
                case TotemGrade.Mythic:
                    return "신화";
                default:
                    return string.Empty;
            }
        }

        private static double GetGradeEffectMultiplier(TotemGrade grade)
        {
            switch (grade)
            {
                case TotemGrade.Common:
                    return 1.00d;
                case TotemGrade.Uncommon:
                    return 1.25d;
                case TotemGrade.Rare:
                    return 1.60d;
                case TotemGrade.Epic:
                    return 2.05d;
                case TotemGrade.Legendary:
                    return 2.65d;
                case TotemGrade.Mythic:
                    return 3.40d;
                default:
                    return 1d;
            }
        }

        private static double GetGradeCostMultiplier(TotemGrade grade)
        {
            switch (grade)
            {
                case TotemGrade.Common:
                    return 1.00d;
                case TotemGrade.Uncommon:
                    return 1.45d;
                case TotemGrade.Rare:
                    return 2.15d;
                case TotemGrade.Epic:
                    return 3.25d;
                case TotemGrade.Legendary:
                    return 5.00d;
                case TotemGrade.Mythic:
                    return 8.00d;
                default:
                    return 1d;
            }
        }
    }
}
