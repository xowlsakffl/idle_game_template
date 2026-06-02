using System;
using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.Battle
{
    internal static partial class FacilityProgressionService
    {
        public static bool AutoAssignFacility(
            FacilityState state,
            IReadOnlyList<FacilityState> facilities,
            IEnumerable<HeroState> heroes,
            Action<FacilityState> refreshProduction,
            out string battleLog)
        {
            battleLog = string.Empty;
            if (state == null)
            {
                return false;
            }

            refreshProduction?.Invoke(state);
            HashSet<string> usedHeroIds = FacilityProductionService.GetAssignedHeroIdsExcept(facilities, state.Definition.Id);
            FacilityProductionService.FillEmptyAssignments(state, heroes, usedHeroIds);
            battleLog = state.Definition.DisplayName + " 추천 배치";
            return true;
        }

        public static int AutoAssignAllFacilities(
            IReadOnlyList<FacilityState> facilities,
            IEnumerable<HeroState> heroes,
            Action<FacilityState> refreshProduction,
            List<FacilityState> changedFacilities,
            out string battleLog)
        {
            battleLog = "시설 전체 추천 배치";
            changedFacilities?.Clear();
            if (facilities == null)
            {
                return 0;
            }

            var usedHeroIds = new HashSet<string>();
            foreach (FacilityState state in facilities)
            {
                refreshProduction?.Invoke(state);
                FacilityProductionService.FillEmptyAssignments(state, heroes, usedHeroIds);
                changedFacilities?.Add(state);
            }

            return changedFacilities != null ? changedFacilities.Count : facilities.Count;
        }

        public static bool ClearFacilityAssignments(FacilityState state, Action<FacilityState> refreshProduction, out string battleLog)
        {
            battleLog = string.Empty;
            if (state == null)
            {
                return false;
            }

            refreshProduction?.Invoke(state);
            state.ClearAssignments();
            battleLog = state.Definition.DisplayName + " 배치 해제";
            return true;
        }

        public static int ClearAllFacilityAssignments(
            IReadOnlyList<FacilityState> facilities,
            Action<FacilityState> refreshProduction,
            List<FacilityState> changedFacilities,
            out string battleLog)
        {
            battleLog = "시설 배치 모두 해제";
            changedFacilities?.Clear();
            if (facilities == null)
            {
                return 0;
            }

            foreach (FacilityState state in facilities)
            {
                refreshProduction?.Invoke(state);
                state?.ClearAssignments();
                if (state != null)
                {
                    changedFacilities?.Add(state);
                }
            }

            return changedFacilities != null ? changedFacilities.Count : facilities.Count;
        }
    }
}
