using System;
using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.Battle
{
    internal static class FacilityCommandService
    {
        internal readonly struct FacilityCollectResult
        {
            public FacilityCollectResult(int collectedCount, string rewardLog, string battleLog)
            {
                CollectedCount = collectedCount;
                RewardLog = rewardLog ?? string.Empty;
                BattleLog = battleLog ?? string.Empty;
            }

            public int CollectedCount { get; }
            public string RewardLog { get; }
            public string BattleLog { get; }
        }

        public static FacilityState GetFacilityState(
            IReadOnlyDictionary<string, FacilityState> facilitiesById,
            string facilityId)
        {
            return !string.IsNullOrEmpty(facilityId)
                && facilitiesById != null
                && facilitiesById.TryGetValue(facilityId, out FacilityState state)
                    ? state
                    : null;
        }

        public static GameNumber GetProductionPerHour(
            FacilityState state,
            Func<FacilityState, double> heroBonusPercentResolver)
        {
            return FacilityProductionService.GetProductionPerHour(state, heroBonusPercentResolver);
        }

        public static GameNumber GetMaxStoredAmount(
            FacilityState state,
            Func<FacilityState, double> heroBonusPercentResolver)
        {
            return FacilityProductionService.GetMaxStoredAmount(GetProductionPerHour(state, heroBonusPercentResolver));
        }

        public static double GetHeroBonusPercent(FacilityState state, Func<string, HeroState> findHero)
        {
            return FacilityProductionService.GetHeroBonusPercent(state, findHero);
        }

        public static bool RefreshProduction(
            FacilityState state,
            Func<FacilityState, double> heroBonusPercentResolver,
            long nowTicks)
        {
            return FacilityProductionService.RefreshProduction(
                state,
                facility => GetProductionPerHour(facility, heroBonusPercentResolver),
                nowTicks);
        }

        public static bool TryUpgradeFacility(
            IReadOnlyDictionary<string, FacilityState> facilitiesById,
            string facilityId,
            CurrencyWallet wallet,
            Func<FacilityState, double> heroBonusPercentResolver,
            long nowTicks,
            out FacilityState state,
            out string battleLog)
        {
            state = GetFacilityState(facilitiesById, facilityId);
            if (state == null)
            {
                battleLog = string.Empty;
                return false;
            }

            RefreshProduction(state, heroBonusPercentResolver, nowTicks);
            return FacilityProgressionService.TryUpgradeFacility(state, wallet, out battleLog);
        }

        public static bool TryCollectFacility(
            IReadOnlyDictionary<string, FacilityState> facilitiesById,
            string facilityId,
            Func<FacilityState, double> heroBonusPercentResolver,
            Func<FacilityState, GameNumber, string> grantReward,
            long nowTicks,
            out FacilityState state,
            out string rewardLog,
            out string battleLog)
        {
            state = GetFacilityState(facilitiesById, facilityId);
            if (state == null)
            {
                rewardLog = string.Empty;
                battleLog = string.Empty;
                return false;
            }

            RefreshProduction(state, heroBonusPercentResolver, nowTicks);
            return FacilityProgressionService.TryCollectFacility(
                state,
                grantReward,
                nowTicks,
                out rewardLog,
                out battleLog);
        }

        public static FacilityCollectResult CollectAllFacilities(
            IReadOnlyList<FacilityState> facilities,
            Func<FacilityState, double> heroBonusPercentResolver,
            Func<FacilityState, GameNumber, string> grantReward,
            List<FacilityState> changedFacilities,
            long nowTicks)
        {
            FacilityProgressionService.FacilityCollectResult result = FacilityProgressionService.CollectAllFacilities(
                facilities,
                state => RefreshProduction(state, heroBonusPercentResolver, nowTicks),
                grantReward,
                changedFacilities,
                nowTicks);
            return new FacilityCollectResult(result.CollectedCount, result.RewardLog, result.BattleLog);
        }

        public static string GrantReward(
            FacilityState state,
            GameNumber amount,
            CurrencyWallet wallet,
            IReadOnlyList<RuneState> runes,
            Func<int, int> randomIndex,
            Action<RuneState> saveRuneState)
        {
            return FacilityProgressionService.GrantFacilityReward(
                state,
                amount,
                wallet,
                runes,
                randomIndex,
                saveRuneState);
        }

        public static bool AutoAssignFacility(
            IReadOnlyDictionary<string, FacilityState> facilitiesById,
            string facilityId,
            IReadOnlyList<FacilityState> facilities,
            IEnumerable<HeroState> heroes,
            Func<FacilityState, double> heroBonusPercentResolver,
            long nowTicks,
            out FacilityState state,
            out string battleLog)
        {
            state = GetFacilityState(facilitiesById, facilityId);
            if (state == null)
            {
                battleLog = string.Empty;
                return false;
            }

            return FacilityProgressionService.AutoAssignFacility(
                state,
                facilities,
                heroes,
                facility => RefreshProduction(facility, heroBonusPercentResolver, nowTicks),
                out battleLog);
        }

        public static int AutoAssignAllFacilities(
            IReadOnlyList<FacilityState> facilities,
            IEnumerable<HeroState> heroes,
            Func<FacilityState, double> heroBonusPercentResolver,
            List<FacilityState> changedFacilities,
            long nowTicks,
            out string battleLog)
        {
            return FacilityProgressionService.AutoAssignAllFacilities(
                facilities,
                heroes,
                state => RefreshProduction(state, heroBonusPercentResolver, nowTicks),
                changedFacilities,
                out battleLog);
        }

        public static bool ClearFacilityAssignments(
            IReadOnlyDictionary<string, FacilityState> facilitiesById,
            string facilityId,
            Func<FacilityState, double> heroBonusPercentResolver,
            long nowTicks,
            out FacilityState state,
            out string battleLog)
        {
            state = GetFacilityState(facilitiesById, facilityId);
            if (state == null)
            {
                battleLog = string.Empty;
                return false;
            }

            return FacilityProgressionService.ClearFacilityAssignments(
                state,
                facility => RefreshProduction(facility, heroBonusPercentResolver, nowTicks),
                out battleLog);
        }

        public static int ClearAllFacilityAssignments(
            IReadOnlyList<FacilityState> facilities,
            Func<FacilityState, double> heroBonusPercentResolver,
            List<FacilityState> changedFacilities,
            long nowTicks,
            out string battleLog)
        {
            return FacilityProgressionService.ClearAllFacilityAssignments(
                facilities,
                state => RefreshProduction(state, heroBonusPercentResolver, nowTicks),
                changedFacilities,
                out battleLog);
        }

        public static int DebugSimulateFacilityHours(
            IReadOnlyList<FacilityState> facilities,
            float hours,
            Func<FacilityState, double> heroBonusPercentResolver,
            List<FacilityState> changedFacilities,
            long nowTicks,
            out string battleLog)
        {
            return FacilityProgressionService.DebugSimulateFacilityHours(
                facilities,
                hours,
                state => RefreshProduction(state, heroBonusPercentResolver, nowTicks),
                changedFacilities,
                out battleLog);
        }

        public static int DebugLevelUpAllFacilities(
            IReadOnlyList<FacilityState> facilities,
            Func<FacilityState, double> heroBonusPercentResolver,
            List<FacilityState> changedFacilities,
            long nowTicks,
            out string battleLog)
        {
            return FacilityProgressionService.DebugLevelUpAllFacilities(
                facilities,
                state => RefreshProduction(state, heroBonusPercentResolver, nowTicks),
                changedFacilities,
                out battleLog);
        }
    }
}
