using System.Collections.Generic;
using System;
using UnityEngine;

namespace IdleGame.Data
{

    [Serializable]
    public sealed partial class TotemDefinition
    {
        public const int MaxLevel = 100;

        public TotemDefinition(
            string id,
            string displayName,
            string icon,
            string role,
            TotemArchetype archetype,
            bool startUnlocked = true)
        {
            Id = id;
            DisplayName = displayName;
            Icon = icon;
            Role = role;
            Archetype = archetype;
            StartUnlocked = startUnlocked;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Icon { get; }
        public string Role { get; }
        public TotemArchetype Archetype { get; }
        public bool StartUnlocked { get; }

        public int GetLevelUpCost(int level, TotemGrade grade)
        {
            if (level >= MaxLevel)
            {
                return 0;
            }

            double cost = Math.Ceiling(6d * GetGradeCostMultiplier(grade) * Math.Pow(Mathf.Max(1, level), 1.22d));
            if (double.IsNaN(cost) || cost <= 1d)
            {
                return 1;
            }

            return cost >= GameData.MaxIntBalanceValue
                ? GameData.MaxIntBalanceValue
                : Mathf.Max(1, (int)cost);
        }

        public int GetPromoteCost(TotemGrade grade)
        {
            switch (grade)
            {
                case TotemGrade.Common:
                    return 300;
                case TotemGrade.Uncommon:
                    return 900;
                case TotemGrade.Rare:
                    return 2700;
                case TotemGrade.Epic:
                    return 8100;
                case TotemGrade.Legendary:
                    return 24000;
                default:
                    return 0;
            }
        }

        public string GetDisplayName(TotemGrade grade)
        {
            return GetGradePrefix(grade) + " " + DisplayName;
        }



    }

}
