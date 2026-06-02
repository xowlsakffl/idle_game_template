using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Save;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static partial class FormationLoadoutService
    {
        private static TotemState FindTotem(IReadOnlyDictionary<string, TotemState> totemsById, string totemId)
        {
            return !string.IsNullOrEmpty(totemId) && totemsById != null && totemsById.TryGetValue(totemId, out TotemState state)
                ? state
                : null;
        }

        private static RuneState FindRune(IReadOnlyDictionary<string, RuneState> runesById, string runeId)
        {
            return !string.IsNullOrEmpty(runeId) && runesById != null && runesById.TryGetValue(runeId, out RuneState state)
                ? state
                : null;
        }
    }
}
