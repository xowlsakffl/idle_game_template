using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Progression;
using IdleGame.Save;
using IdleGame.Speed;

namespace IdleGame.Battle
{
    public sealed partial class BattleManager
    {
        public FacilityState GetFacilityState(string facilityId)
        {
            return FacilityCommandService.GetFacilityState(facilitiesById, facilityId);
        }

        public GameNumber GetFacilityProductionPerHour(string facilityId)
        {
            FacilityState state = GetFacilityState(facilityId);
            if (state == null)
            {
                return GameNumber.Zero;
            }

            RefreshFacilityProduction(state, false);
            return FacilityCommandService.GetProductionPerHour(state, GetFacilityHeroBonusPercent);
        }

        public GameNumber GetFacilityMaxStoredAmount(string facilityId)
        {
            FacilityState state = GetFacilityState(facilityId);
            return state != null
                ? FacilityCommandService.GetMaxStoredAmount(state, GetFacilityHeroBonusPercent)
                : GameNumber.Zero;
        }

        public double GetFacilityHeroBonusPercent(string facilityId)
        {
            FacilityState state = GetFacilityState(facilityId);
            return state != null ? GetFacilityHeroBonusPercent(state) : 0d;
        }

        public bool TryUpgradeFacility(string facilityId)
        {
            if (!IsReady())
            {
                return false;
            }

            bool upgraded = FacilityCommandService.TryUpgradeFacility(
                facilitiesById,
                facilityId,
                wallet,
                GetFacilityHeroBonusPercent,
                DateTime.UtcNow.Ticks,
                out FacilityState state,
                out string battleLog);
            return ApplyLoggedChange(
                upgraded,
                battleLog,
                () =>
                {
                    SaveFacilityState(state, true);
                    NotifyChanged(BattleChangeFlags.Facility);
                });
        }

        public bool CollectFacility(string facilityId)
        {
            if (!IsReady())
            {
                return false;
            }

            bool collected = FacilityCommandService.TryCollectFacility(
                facilitiesById,
                facilityId,
                GetFacilityHeroBonusPercent,
                GrantFacilityReward,
                DateTime.UtcNow.Ticks,
                out FacilityState state,
                out string rewardLog,
                out string battleLog);
            return ApplyLoggedChange(
                collected,
                battleLog,
                () =>
                {
                    SaveFacilityState(state, true);
                    LastRewardLog = rewardLog;
                    NotifyChanged(BattleChangeFlags.Facility);
                });
        }

        public int CollectAllFacilities()
        {
            if (!IsReady())
            {
                return 0;
            }

            var changedFacilities = new List<FacilityState>();
            FacilityCommandService.FacilityCollectResult result = FacilityCommandService.CollectAllFacilities(
                facilities,
                GetFacilityHeroBonusPercent,
                GrantFacilityReward,
                changedFacilities,
                DateTime.UtcNow.Ticks);
            SaveFacilityStates(changedFacilities, true);
            LastRewardLog = result.RewardLog;
            ApplyBattleLog(result.BattleLog);
            NotifyChanged(BattleChangeFlags.Facility);
            return result.CollectedCount;
        }

        public bool AutoAssignFacility(string facilityId)
        {
            if (!IsReady())
            {
                return false;
            }

            bool assigned = FacilityCommandService.AutoAssignFacility(
                facilitiesById,
                facilityId,
                facilities,
                Heroes,
                GetFacilityHeroBonusPercent,
                DateTime.UtcNow.Ticks,
                out FacilityState state,
                out string battleLog);
            return ApplyLoggedChange(
                assigned,
                battleLog,
                () =>
                {
                    SaveFacilityState(state, true);
                    NotifyChanged(BattleChangeFlags.Facility);
                });
        }

        public void AutoAssignAllFacilities()
        {
            if (!IsReady())
            {
                return;
            }

            var changedFacilities = new List<FacilityState>();
            int changedCount = FacilityCommandService.AutoAssignAllFacilities(
                facilities,
                Heroes,
                GetFacilityHeroBonusPercent,
                changedFacilities,
                DateTime.UtcNow.Ticks,
                out string battleLog);
            ApplyFacilityBatchChange(changedCount, changedFacilities, battleLog);
        }

        public void ClearFacilityAssignments(string facilityId)
        {
            if (!FacilityCommandService.ClearFacilityAssignments(
                facilitiesById,
                facilityId,
                GetFacilityHeroBonusPercent,
                DateTime.UtcNow.Ticks,
                out FacilityState state,
                out string battleLog))
            {
                return;
            }

            SaveFacilityState(state, true);
            ApplyBattleLog(battleLog);
            NotifyChanged(BattleChangeFlags.Facility);
        }

        public void ClearAllFacilityAssignments()
        {
            if (!IsReady())
            {
                return;
            }

            var changedFacilities = new List<FacilityState>();
            int changedCount = FacilityCommandService.ClearAllFacilityAssignments(
                facilities,
                GetFacilityHeroBonusPercent,
                changedFacilities,
                DateTime.UtcNow.Ticks,
                out string battleLog);
            ApplyFacilityBatchChange(changedCount, changedFacilities, battleLog);
        }

        public void DebugSimulateFacilityHours(float hours)
        {
            if (!IsReady() || hours <= 0f)
            {
                return;
            }

            var changedFacilities = new List<FacilityState>();
            int changedCount = FacilityCommandService.DebugSimulateFacilityHours(
                facilities,
                hours,
                GetFacilityHeroBonusPercent,
                changedFacilities,
                DateTime.UtcNow.Ticks,
                out string battleLog);
            ApplyFacilityBatchChange(changedCount, changedFacilities, battleLog);
        }

        public void DebugLevelUpAllFacilities()
        {
            if (!IsReady())
            {
                return;
            }

            var changedFacilities = new List<FacilityState>();
            int changedCount = FacilityCommandService.DebugLevelUpAllFacilities(
                facilities,
                GetFacilityHeroBonusPercent,
                changedFacilities,
                DateTime.UtcNow.Ticks,
                out string battleLog);
            ApplyFacilityBatchChange(changedCount, changedFacilities, battleLog);
        }

        private void ApplyFacilityBatchChange(int changedCount, List<FacilityState> changedFacilities, string battleLog)
        {
            if (changedCount <= 0)
            {
                return;
            }

            SaveFacilityStates(changedFacilities, true);
            ApplyBattleLog(battleLog);
            NotifyChanged(BattleChangeFlags.Facility);
        }

        private void RefreshFacilityProduction(FacilityState state, bool save)
        {
            bool changed = FacilityCommandService.RefreshProduction(state, GetFacilityHeroBonusPercent, DateTime.UtcNow.Ticks);
            if (changed && save)
            {
                SaveFacilityState(state, true);
            }
        }

        private double GetFacilityHeroBonusPercent(FacilityState state)
        {
            return FacilityCommandService.GetHeroBonusPercent(state, FindHero);
        }

        private string GrantFacilityReward(FacilityState state, GameNumber amount)
        {
            return FacilityCommandService.GrantReward(
                state,
                amount,
                wallet,
                runes,
                random.Next,
                rune => SaveRuneState(rune, false));
        }
    }
}
