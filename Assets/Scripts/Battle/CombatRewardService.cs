using System;
using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.Battle
{
    internal static class CombatRewardService
    {
        internal readonly struct RewardAmounts
        {
            public RewardAmounts(GameNumber gold, GameNumber heroExpItems, GameNumber accountExp, GameNumber fortressExp)
            {
                Gold = gold;
                HeroExpItems = heroExpItems;
                AccountExp = accountExp;
                FortressExp = fortressExp;
            }

            public GameNumber Gold { get; }
            public GameNumber HeroExpItems { get; }
            public GameNumber AccountExp { get; }
            public GameNumber FortressExp { get; }
        }

        internal readonly struct FacilityMaterialReward
        {
            public FacilityMaterialReward(long wood, long brick, long iron)
            {
                Wood = Math.Max(0L, wood);
                Brick = Math.Max(0L, brick);
                Iron = Math.Max(0L, iron);
            }

            public long Wood { get; }
            public long Brick { get; }
            public long Iron { get; }
            public bool HasAny => Wood > 0L || Brick > 0L || Iron > 0L;
        }

        public static RewardAmounts CalculateBossClearReward(
            StageDefinition stage,
            double goldMultiplier,
            double accountExpMultiplier)
        {
            if (stage == null)
            {
                return new RewardAmounts(GameNumber.Zero, GameNumber.Zero, GameNumber.Zero, GameNumber.Zero);
            }

            return new RewardAmounts(
                GameNumber.Floor(GameData.GetBossClearGold(stage) * goldMultiplier),
                GameNumber.Zero,
                GetAccountExperienceReward(stage, true, accountExpMultiplier),
                FortressCombatService.GetExperienceReward(stage, true));
        }

        public static RewardAmounts CalculateEnemyDefeatReward(
            StageDefinition stage,
            double goldMultiplier,
            double heroExpMultiplier,
            double accountExpMultiplier)
        {
            if (stage == null)
            {
                return new RewardAmounts(GameNumber.Zero, GameNumber.Zero, GameNumber.Zero, GameNumber.Zero);
            }

            return new RewardAmounts(
                GameNumber.Floor(GameData.GetEnemyGold(stage) * goldMultiplier),
                GameNumber.Floor(GameData.GetEnemyHeroExpItem(stage) * heroExpMultiplier),
                GetAccountExperienceReward(stage, false, accountExpMultiplier),
                FortressCombatService.GetExperienceReward(stage, false));
        }

        public static string ApplyReward(
            RewardAmounts reward,
            bool includeHeroExp,
            CurrencyWallet wallet,
            Action<GameNumber> addAccountExperience,
            Action<GameNumber> addFortressExperience)
        {
            string rewardLog = includeHeroExp
                ? BuildEnemyRewardLog(reward)
                : BuildBossRewardLog(reward);
            if (wallet == null)
            {
                return rewardLog;
            }

            wallet.AddGold(reward.Gold);
            if (includeHeroExp)
            {
                wallet.AddHeroExpItem(reward.HeroExpItems);
            }

            addAccountExperience?.Invoke(reward.AccountExp);
            addFortressExperience?.Invoke(reward.FortressExp);
            return rewardLog;
        }

        public static string ApplyBossClearReward(
            StageDefinition stage,
            CurrencyWallet wallet,
            Action<GameNumber> addAccountExperience,
            Action<GameNumber> addFortressExperience,
            double goldMultiplier,
            double accountExpMultiplier,
            Func<double> random01)
        {
            RewardAmounts reward = CalculateBossClearReward(stage, goldMultiplier, accountExpMultiplier);
            return ApplyReward(reward, false, wallet, addAccountExperience, addFortressExperience)
                + GrantHuntingFacilityMaterials(stage, true, wallet, random01);
        }

        public static string ApplyEnemyDefeatReward(
            StageDefinition stage,
            CurrencyWallet wallet,
            Action<GameNumber> addAccountExperience,
            Action<GameNumber> addFortressExperience,
            double goldMultiplier,
            double heroExpMultiplier,
            double accountExpMultiplier,
            Func<double> random01)
        {
            RewardAmounts reward = CalculateEnemyDefeatReward(stage, goldMultiplier, heroExpMultiplier, accountExpMultiplier);
            return ApplyReward(reward, true, wallet, addAccountExperience, addFortressExperience)
                + GrantHuntingFacilityMaterials(stage, false, wallet, random01);
        }

        public static float GetPetGoldBonusMultiplier(IReadOnlyList<PetState> pets)
        {
            float bonus = 1f;
            if (pets == null)
            {
                return bonus;
            }

            foreach (PetState pet in pets)
            {
                if (pet != null)
                {
                    bonus += pet.Definition.GoldBonusPercent;
                }
            }

            return bonus;
        }

        public static FacilityMaterialReward RollHuntingFacilityMaterials(StageDefinition stage, bool boss, Func<double> random01)
        {
            if (stage == null)
            {
                return new FacilityMaterialReward(0L, 0L, 0L);
            }

            int stageIndex = GameData.GetStageIndex(stage.Id) + 1;
            if (boss)
            {
                return new FacilityMaterialReward(
                    8L + stageIndex,
                    stageIndex >= 8 ? 2L + stageIndex / 5L : 1L,
                    stageIndex >= 18 ? 1L + stageIndex / 12L : 0L);
            }

            double woodChance = Math.Min(0.18d, 0.04d + stageIndex * 0.002d);
            double roll = random01?.Invoke() ?? 1d;
            return roll < woodChance
                ? new FacilityMaterialReward(1L + stageIndex / 25L, 0L, 0L)
                : new FacilityMaterialReward(0L, 0L, 0L);
        }

        public static string BuildBossRewardLog(RewardAmounts reward)
        {
            return "+" + NumberFormatter.Format(reward.Gold) + " 골드"
                + ", +" + NumberFormatter.Format(reward.AccountExp) + " Account EXP"
                + ", 요새 EXP +" + NumberFormatter.Format(reward.FortressExp);
        }

        public static string BuildEnemyRewardLog(RewardAmounts reward)
        {
            return "+" + NumberFormatter.Format(reward.Gold) + " 골드"
                + ", +" + NumberFormatter.Format(reward.HeroExpItems) + " EXP"
                + ", +" + NumberFormatter.Format(reward.AccountExp) + " Account EXP"
                + ", 요새 EXP +" + NumberFormatter.Format(reward.FortressExp);
        }

        public static string BuildHuntingFacilityMaterialLog(FacilityMaterialReward reward)
        {
            if (!reward.HasAny)
            {
                return string.Empty;
            }

            var parts = new List<string>();
            if (reward.Wood > 0L)
            {
                parts.Add("목재 +" + reward.Wood);
            }

            if (reward.Brick > 0L)
            {
                parts.Add("벽돌 +" + reward.Brick);
            }

            if (reward.Iron > 0L)
            {
                parts.Add("철재 +" + reward.Iron);
            }

            return " / " + string.Join(", ", parts);
        }

        public static string GrantHuntingFacilityMaterials(
            StageDefinition stage,
            bool boss,
            CurrencyWallet wallet,
            Func<double> random01)
        {
            if (stage == null || wallet == null)
            {
                return string.Empty;
            }

            FacilityMaterialReward reward = RollHuntingFacilityMaterials(stage, boss, random01);
            if (!reward.HasAny)
            {
                return string.Empty;
            }

            wallet.AddFacilityMaterials(reward.Wood, reward.Brick, reward.Iron);
            return BuildHuntingFacilityMaterialLog(reward);
        }

        public static string BuildStageFirstClearRewardText(StageClearReward reward)
        {
            var parts = new List<string>();
            if (reward.HeroSummonTickets > 0)
            {
                parts.Add("영웅권 +" + reward.HeroSummonTickets);
            }

            if (reward.EquipmentSummonTickets > 0)
            {
                parts.Add("장비권 +" + reward.EquipmentSummonTickets);
            }

            if (reward.Ruby > 0)
            {
                parts.Add("루비 +" + reward.Ruby);
            }

            if (reward.HeroExpItems > 0)
            {
                parts.Add("경험치책 +" + reward.HeroExpItems);
            }

            if (reward.EquipmentExpItems > 0)
            {
                parts.Add("장비책 +" + reward.EquipmentExpItems);
            }

            if (reward.HeroTranscendStones > 0)
            {
                parts.Add("초월석 +" + reward.HeroTranscendStones);
            }

            return string.Join(", ", parts);
        }

        private static GameNumber GetAccountExperienceReward(StageDefinition stage, bool boss, double accountExpMultiplier)
        {
            double baseReward = 2d + stage.Chapter * 0.5d + stage.Number * 0.08d;
            if (boss)
            {
                baseReward *= 30d;
            }

            baseReward *= accountExpMultiplier;
            return GameNumber.Floor(GameNumber.FromDouble(Math.Max(1d, baseReward)));
        }
    }
}
