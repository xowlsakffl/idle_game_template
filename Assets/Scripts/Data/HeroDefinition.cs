using System.Collections.Generic;
using System;
using UnityEngine;

namespace IdleGame.Data
{
    [Serializable]
    public sealed partial class HeroDefinition
    {
        public const int MaxStars = 15;
        public const int MaxTranscendSlots = 5;
        public const int LevelPerStar = 50;
        public const int MaxLevelAtMaxStars = (MaxStars + 1) * LevelPerStar;
        private const float FiveStarPassiveBoostMultiplier = 1.5f;
        private const float TenStarAllStatMultiplier = 1.1f;
        private static readonly int[] TranscendSlotRequiredStars = { 0, 1, 3, 7, 11 };

        public HeroDefinition(
            string id,
            string displayName,
            string role,
            HeroRarity rarity,
            HeroTrait trait,
            int baseAttack,
            int baseHp,
            float attackSpeed,
            float moveSpeed,
            int attackPerLevel,
            int hpPerLevel,
            HeroPassiveStat passiveStat,
            float passiveValuePercent,
            bool startUnlocked = false)
        {
            Id = id;
            DisplayName = displayName;
            Role = role;
            Rarity = rarity;
            Trait = trait;
            BaseAttack = baseAttack;
            BaseHp = baseHp;
            AttackSpeed = Mathf.Max(0.1f, attackSpeed);
            MoveSpeed = Mathf.Max(0.1f, moveSpeed);
            AttackPerLevel = attackPerLevel;
            HpPerLevel = hpPerLevel;
            PassiveStat = passiveStat;
            PassiveValuePercent = GetBalancedPassiveValue(rarity, passiveValuePercent);
            StartUnlocked = startUnlocked;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Role { get; }
        public HeroRarity Rarity { get; }
        public HeroTrait Trait { get; }
        public int BaseAttack { get; }
        public int BaseHp { get; }
        public float AttackSpeed { get; }
        public float MoveSpeed { get; }
        public float AttackInterval => 1f / AttackSpeed;
        public int AttackPerLevel { get; }
        public int HpPerLevel { get; }
        public HeroPassiveStat PassiveStat { get; }
        public float PassiveValuePercent { get; }
        public bool StartUnlocked { get; }
        public string RarityLabel => GetRarityLabel(Rarity);
        public string PassiveLabel => GetPassiveLabel(PassiveStat) + " +" + PassiveValuePercent.ToString("0.#") + "%";

        public int GetAttackPower(int level)
        {
            return GetAttackPower(level, 0);
        }

        public int GetAttackPower(int level, int stars)
        {
            int levelAttack = BaseAttack + Mathf.Max(0, level - 1) * AttackPerLevel;
            float starMultiplier = 1f + Mathf.Clamp(stars, 0, MaxStars) * 0.06f;
            return Mathf.Clamp(Mathf.FloorToInt(levelAttack
                * starMultiplier
                * GetRarityStatMultiplier(Rarity)
                * GetPassiveMultiplier(HeroPassiveStat.AttackPower, stars)
                * GetTenStarMultiplier(stars)), 1, GameData.MaxIntBalanceValue);
        }

        public int GetMaxHp(int level, int stars)
        {
            int levelHp = BaseHp + Mathf.Max(0, level - 1) * HpPerLevel;
            float starMultiplier = 1f + Mathf.Clamp(stars, 0, MaxStars) * 0.055f;
            return Mathf.Clamp(Mathf.FloorToInt(levelHp
                * starMultiplier
                * GetRarityStatMultiplier(Rarity)
                * GetPassiveMultiplier(HeroPassiveStat.MaxHp, stars)
                * GetTenStarMultiplier(stars)), 1, GameData.MaxIntBalanceValue);
        }

        public float GetAttackSpeed(int stars)
        {
            return AttackSpeed
                * GetRaritySpeedMultiplier(Rarity)
                * GetPassiveMultiplier(HeroPassiveStat.AttackSpeed, stars)
                * GetTenStarMultiplier(stars);
        }

        public float GetMoveSpeed(int stars)
        {
            return MoveSpeed
                * GetRarityMoveMultiplier(Rarity)
                * GetPassiveMultiplier(HeroPassiveStat.MoveSpeed, stars)
                * GetTenStarMultiplier(stars);
        }

        public int GetLevelUpCost(int level)
        {
            double cost = 18d * Math.Pow(Mathf.Max(1, level), 1.22d) * GetRarityLevelCostMultiplier(Rarity);
            if (double.IsNaN(cost) || cost <= 1d)
            {
                return 1;
            }

            if (cost >= GameData.MaxIntBalanceValue)
            {
                return GameData.MaxIntBalanceValue;
            }

            return Math.Max(1, (int)Math.Floor(cost));
        }

        public int GetMaxLevel(int stars)
        {
            int effectiveStars = Mathf.Clamp(stars, 0, MaxStars) + 1;
            return effectiveStars * LevelPerStar;
        }

        public int GetStarUpCost(int currentStars)
        {
            if (currentStars >= MaxStars)
            {
                return int.MaxValue;
            }

            return GetBaseShardStep(Rarity) * (Mathf.Clamp(currentStars, 0, MaxStars - 1) + 1);
        }

        public int GetSummonShardReward()
        {
            return 1;
        }

        public static int GetTranscendRequiredStars(int slotIndex)
        {
            int clampedIndex = Mathf.Clamp(slotIndex, 0, TranscendSlotRequiredStars.Length - 1);
            return TranscendSlotRequiredStars[clampedIndex];
        }

    }

}
