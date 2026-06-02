using System;
using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.Battle
{
    internal static partial class FacilityProgressionService
    {
        private static long GameNumberToLong(GameNumber value)
        {
            double clamped = GameData.ClampVisibleNumber(value.ToDoubleClamped());
            if (clamped <= 0d)
            {
                return 0L;
            }

            if (clamped >= long.MaxValue)
            {
                return long.MaxValue;
            }

            return GameData.ClampCount((long)Math.Floor(clamped));
        }

        private static void GrantRunesFromBoxes(
            long boxes,
            IReadOnlyList<RuneState> runes,
            Func<int, int> randomIndex,
            Action<RuneState> saveRuneState)
        {
            if (boxes <= 0 || runes == null || runes.Count <= 0)
            {
                return;
            }

            for (long i = 0; i < boxes; i++)
            {
                int index = randomIndex != null ? randomIndex(runes.Count) : 0;
                RuneState state = runes[Math.Max(0, Math.Min(runes.Count - 1, index))];
                state.AddCount(RuneGrade.Common, 1);
                saveRuneState?.Invoke(state);
            }
        }
    }
}
