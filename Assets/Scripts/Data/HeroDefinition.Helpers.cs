using System;
using UnityEngine;

namespace IdleGame.Data
{
    public sealed partial class HeroDefinition
    {
        private float GetPassiveMultiplier(HeroPassiveStat stat, int stars)
        {
            if (PassiveStat != stat)
            {
                return 1f;
            }

            float passivePercent = PassiveValuePercent;
            if (stars >= 5)
            {
                passivePercent *= FiveStarPassiveBoostMultiplier;
            }

            return 1f + passivePercent / 100f;
        }

        private static float GetTenStarMultiplier(int stars)
        {
            return stars >= 10 ? TenStarAllStatMultiplier : 1f;
        }

        private static float GetBalancedPassiveValue(HeroRarity rarity, float rawValue)
        {
            float scaledValue = Mathf.Max(0f, rawValue) * 0.25f;
            switch (rarity)
            {
                case HeroRarity.Common:
                    return Mathf.Clamp(scaledValue, 0.5f, 1.5f);
                case HeroRarity.Uncommon:
                    return Mathf.Clamp(scaledValue, 0.8f, 2.2f);
                case HeroRarity.Rare:
                    return Mathf.Clamp(scaledValue, 1.2f, 3.0f);
                case HeroRarity.Epic:
                    return Mathf.Clamp(scaledValue, 1.8f, 4.0f);
                case HeroRarity.Legendary:
                    return Mathf.Clamp(scaledValue, 2.5f, 4.8f);
                case HeroRarity.Mythic:
                    return Mathf.Clamp(scaledValue, 3.2f, 5.8f);
                default:
                    return scaledValue;
            }
        }

        private static float GetRarityStatMultiplier(HeroRarity rarity)
        {
            switch (rarity)
            {
                case HeroRarity.Common:
                    return 1.00f;
                case HeroRarity.Uncommon:
                    return 1.06f;
                case HeroRarity.Rare:
                    return 1.14f;
                case HeroRarity.Epic:
                    return 1.26f;
                case HeroRarity.Legendary:
                    return 1.42f;
                case HeroRarity.Mythic:
                    return 1.62f;
                default:
                    return 1f;
            }
        }

        private static float GetRaritySpeedMultiplier(HeroRarity rarity)
        {
            switch (rarity)
            {
                case HeroRarity.Common:
                    return 1.00f;
                case HeroRarity.Uncommon:
                    return 1.025f;
                case HeroRarity.Rare:
                    return 1.055f;
                case HeroRarity.Epic:
                    return 1.09f;
                case HeroRarity.Legendary:
                    return 1.13f;
                case HeroRarity.Mythic:
                    return 1.18f;
                default:
                    return 1f;
            }
        }

        private static float GetRarityMoveMultiplier(HeroRarity rarity)
        {
            switch (rarity)
            {
                case HeroRarity.Common:
                    return 1.00f;
                case HeroRarity.Uncommon:
                    return 1.015f;
                case HeroRarity.Rare:
                    return 1.035f;
                case HeroRarity.Epic:
                    return 1.06f;
                case HeroRarity.Legendary:
                    return 1.09f;
                case HeroRarity.Mythic:
                    return 1.12f;
                default:
                    return 1f;
            }
        }

        private static int GetBaseShardStep(HeroRarity rarity)
        {
            switch (rarity)
            {
                case HeroRarity.Common:
                    return 5;
                case HeroRarity.Uncommon:
                    return 10;
                case HeroRarity.Rare:
                    return 15;
                case HeroRarity.Epic:
                    return 25;
                case HeroRarity.Legendary:
                    return 40;
                case HeroRarity.Mythic:
                    return 60;
                default:
                    return 10;
            }
        }

        private static double GetRarityLevelCostMultiplier(HeroRarity rarity)
        {
            switch (rarity)
            {
                case HeroRarity.Common:
                    return 1.00d;
                case HeroRarity.Uncommon:
                    return 1.25d;
                case HeroRarity.Rare:
                    return 1.65d;
                case HeroRarity.Epic:
                    return 2.25d;
                case HeroRarity.Legendary:
                    return 3.05d;
                case HeroRarity.Mythic:
                    return 4.10d;
                default:
                    return 1.00d;
            }
        }

        private static string GetRarityLabel(HeroRarity rarity)
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
                    return "전설";
                case HeroRarity.Mythic:
                    return "신화";
                default:
                    return rarity.ToString();
            }
        }

        private static string GetPassiveLabel(HeroPassiveStat stat)
        {
            switch (stat)
            {
                case HeroPassiveStat.AttackPower:
                    return "공격력";
                case HeroPassiveStat.MaxHp:
                    return "체력";
                case HeroPassiveStat.AttackSpeed:
                    return "공속";
                case HeroPassiveStat.MoveSpeed:
                    return "이속";
                default:
                    return stat.ToString();
            }
        }

    }
}
