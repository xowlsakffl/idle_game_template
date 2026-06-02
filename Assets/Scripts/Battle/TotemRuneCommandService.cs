using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Save;

namespace IdleGame.Battle
{
    internal static class TotemRuneCommandService
    {
        public static TotemState GetTotemState(
            IReadOnlyDictionary<string, TotemState> totemsById,
            string totemId)
        {
            return !string.IsNullOrEmpty(totemId)
                && totemsById != null
                && totemsById.TryGetValue(totemId, out TotemState state)
                    ? state
                    : null;
        }

        public static RuneState GetRuneState(
            IReadOnlyDictionary<string, RuneState> runesById,
            string runeId)
        {
            return !string.IsNullOrEmpty(runeId)
                && runesById != null
                && runesById.TryGetValue(runeId, out RuneState state)
                    ? state
                    : null;
        }

        public static bool TryLevelUpTotem(
            IReadOnlyDictionary<string, TotemState> totemsById,
            string totemId,
            CurrencyWallet wallet,
            out TotemState state,
            out string battleLog)
        {
            state = GetTotemState(totemsById, totemId);
            return TotemRuneProgressionService.TryLevelUpTotem(state, wallet, out battleLog);
        }

        public static bool CanPromoteTotemTier(
            IReadOnlyDictionary<string, TotemState> totemsById,
            IReadOnlyList<TotemState> totems,
            string totemId)
        {
            return TotemRuneProgressionService.CanPromoteTotemTier(GetTotemState(totemsById, totemId), totems);
        }

        public static bool TryPromoteTotem(
            IReadOnlyDictionary<string, TotemState> totemsById,
            IReadOnlyList<TotemState> totems,
            string totemId,
            CurrencyWallet wallet,
            List<TotemState> changedTotems,
            out string battleLog)
        {
            TotemState state = GetTotemState(totemsById, totemId);
            return TotemRuneProgressionService.TryPromoteTotem(state, totems, wallet, changedTotems, out battleLog);
        }

        public static int DebugUnlockAllTotems(
            IReadOnlyList<TotemState> totems,
            List<TotemState> changedTotems,
            out string battleLog)
        {
            return TotemRuneProgressionService.DebugUnlockAllTotems(totems, changedTotems, out battleLog);
        }

        public static bool TryPromoteRune(
            IReadOnlyDictionary<string, RuneState> runesById,
            SaveManager saveManager,
            int activePreset,
            string runeId,
            out RuneState state,
            out bool affectsActiveLoadout,
            out string battleLog)
        {
            state = GetRuneState(runesById, runeId);
            bool promoted = TotemRuneProgressionService.TryPromoteRune(state, out bool highestGradeChanged, out battleLog);
            affectsActiveLoadout = promoted
                && highestGradeChanged
                && state != null
                && FormationLoadoutService.IsRuneEquipped(saveManager, runesById, activePreset, state.Definition.Id);
            return promoted;
        }

        public static int PromoteAllRunes(
            IReadOnlyList<RuneState> runes,
            IReadOnlyDictionary<string, RuneState> runesById,
            SaveManager saveManager,
            int activePreset,
            List<RuneState> changedRunes,
            out bool affectsActiveLoadout,
            out string battleLog)
        {
            var highestGradeChangedRuneIds = new HashSet<string>();
            int promotedCount = TotemRuneProgressionService.PromoteAllRunes(
                runes,
                changedRunes,
                highestGradeChangedRuneIds,
                out battleLog);
            affectsActiveLoadout = FormationLoadoutService.HasEquippedRune(
                saveManager,
                runesById,
                activePreset,
                highestGradeChangedRuneIds);
            return promotedCount;
        }

        public static int DebugUnlockAllRunes(
            IReadOnlyList<RuneState> runes,
            List<RuneState> changedRunes,
            out string battleLog)
        {
            return TotemRuneProgressionService.DebugUnlockAllRunes(runes, changedRunes, out battleLog);
        }

        public static int DebugAddRuneItems(
            IReadOnlyList<RuneState> runes,
            int commonRunesPerRune,
            List<RuneState> changedRunes,
            out string battleLog)
        {
            return TotemRuneProgressionService.DebugAddRuneItems(runes, commonRunesPerRune, changedRunes, out battleLog);
        }
    }
}
