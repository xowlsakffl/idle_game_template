using System;
using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.Battle
{
    internal static partial class FacilityProgressionService
    {
        public static int DebugSimulateFacilityHours(
            IReadOnlyList<FacilityState> facilities,
            float hours,
            Action<FacilityState> refreshProduction,
            List<FacilityState> changedFacilities,
            out string battleLog)
        {
            battleLog = string.Empty;
            changedFacilities?.Clear();
            if (facilities == null || hours <= 0f)
            {
                return 0;
            }

            long ticks = TimeSpan.FromHours(hours).Ticks;
            foreach (FacilityState state in facilities)
            {
                if (state == null)
                {
                    continue;
                }

                state.LastUpdateUtcTicks = Math.Max(0L, state.LastUpdateUtcTicks - ticks);
                refreshProduction?.Invoke(state);
                changedFacilities?.Add(state);
            }

            battleLog = "QA: 시설 생산 " + hours.ToString("0.#") + "시간";
            return changedFacilities != null ? changedFacilities.Count : facilities.Count;
        }

        public static int DebugLevelUpAllFacilities(
            IReadOnlyList<FacilityState> facilities,
            Action<FacilityState> refreshProduction,
            List<FacilityState> changedFacilities,
            out string battleLog)
        {
            battleLog = "QA: 모든 시설 Lv.+1";
            changedFacilities?.Clear();
            if (facilities == null)
            {
                return 0;
            }

            foreach (FacilityState state in facilities)
            {
                if (state == null)
                {
                    continue;
                }

                refreshProduction?.Invoke(state);
                if (!state.IsMaxed)
                {
                    state.Level += 1;
                }

                changedFacilities?.Add(state);
            }

            return changedFacilities != null ? changedFacilities.Count : facilities.Count;
        }
    }
}
