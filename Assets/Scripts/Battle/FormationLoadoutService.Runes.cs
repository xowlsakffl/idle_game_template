using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Save;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static partial class FormationLoadoutService
    {
        public static string GetEquippedRuneId(
            SaveManager saveManager,
            IReadOnlyDictionary<string, RuneState> runesById,
            int preset,
            int slot)
        {
            int normalizedPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
            int normalizedSlot = Mathf.Clamp(slot, 1, GameData.MaxRuneSlots);
            string key = SaveKeys.HeroFormationRune(normalizedPreset, normalizedSlot);
            string savedId = saveManager != null
                ? saveManager.LoadString(key, string.Empty)
                : PlayerPrefs.GetString(key, string.Empty);

            if (savedId == UnequippedRuneId)
            {
                return string.Empty;
            }

            RuneState state = FindRune(runesById, savedId);
            return state != null ? state.Definition.Id : string.Empty;
        }

        public static bool IsRuneSlotUnlocked(int accountLevel, int slot)
        {
            int normalizedSlot = Mathf.Clamp(slot, 1, GameData.MaxRuneSlots);
            return accountLevel >= GameData.GetRuneSlotUnlockLevel(normalizedSlot);
        }

        public static bool IsRuneEquipped(
            SaveManager saveManager,
            IReadOnlyDictionary<string, RuneState> runesById,
            int preset,
            string runeId)
        {
            if (string.IsNullOrEmpty(runeId))
            {
                return false;
            }

            for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
            {
                if (GetEquippedRuneId(saveManager, runesById, preset, slot) == runeId)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasEquippedRune(
            SaveManager saveManager,
            IReadOnlyDictionary<string, RuneState> runesById,
            int preset,
            HashSet<string> runeIds)
        {
            if (runeIds == null || runeIds.Count <= 0)
            {
                return false;
            }

            foreach (string runeId in runeIds)
            {
                if (IsRuneEquipped(saveManager, runesById, preset, runeId))
                {
                    return true;
                }
            }

            return false;
        }

        public static void FillActiveUsableRunes(
            SaveManager saveManager,
            IReadOnlyDictionary<string, RuneState> runesById,
            int accountLevel,
            int preset,
            IList<RuneState> activeRunes)
        {
            activeRunes?.Clear();
            if (activeRunes == null)
            {
                return;
            }

            for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
            {
                if (!IsRuneSlotUnlocked(accountLevel, slot))
                {
                    continue;
                }

                RuneState state = FindRune(runesById, GetEquippedRuneId(saveManager, runesById, preset, slot));
                if (state != null && state.Unlocked)
                {
                    activeRunes.Add(state);
                }
            }
        }

        public static bool TrySetRuneForPreset(
            SaveManager saveManager,
            IReadOnlyDictionary<string, RuneState> runesById,
            int accountLevel,
            int preset,
            int slot,
            string runeId,
            out FormationLoadoutChangeResult result)
        {
            RuneState state = FindRune(runesById, runeId);
            if (state == null || !state.Unlocked)
            {
                result = new FormationLoadoutChangeResult(false, preset, slot, "룬 장착 실패: 보유하지 않은 룬");
                return false;
            }

            int normalizedPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
            int normalizedSlot = Mathf.Clamp(slot, 1, GameData.MaxRuneSlots);
            if (!IsRuneSlotUnlocked(accountLevel, normalizedSlot))
            {
                result = new FormationLoadoutChangeResult(
                    false,
                    normalizedPreset,
                    normalizedSlot,
                    normalizedSlot + "번 룬 슬롯은 계정 Lv." + GameData.GetRuneSlotUnlockLevel(normalizedSlot) + "에 해금됩니다.");
                return false;
            }

            for (int i = 1; i <= GameData.MaxRuneSlots; i++)
            {
                if (i != normalizedSlot && GetEquippedRuneId(saveManager, runesById, normalizedPreset, i) == state.Definition.Id)
                {
                    result = new FormationLoadoutChangeResult(false, normalizedPreset, normalizedSlot, "룬 장착 실패: 이미 다른 슬롯에 장착됨");
                    return false;
                }
            }

            saveManager.SaveString(SaveKeys.HeroFormationRune(normalizedPreset, normalizedSlot), state.Definition.Id);
            saveManager.Flush();
            result = new FormationLoadoutChangeResult(
                true,
                normalizedPreset,
                normalizedSlot,
                "프리셋 " + normalizedPreset + " " + normalizedSlot + "번 룬 장착: " + state.Definition.DisplayName);
            return true;
        }

        public static bool TryClearRuneForPreset(SaveManager saveManager, int preset, int slot, out FormationLoadoutChangeResult result)
        {
            int normalizedPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
            int normalizedSlot = Mathf.Clamp(slot, 1, GameData.MaxRuneSlots);
            saveManager.SaveString(SaveKeys.HeroFormationRune(normalizedPreset, normalizedSlot), UnequippedRuneId);
            saveManager.Flush();
            result = new FormationLoadoutChangeResult(true, normalizedPreset, normalizedSlot, "프리셋 " + normalizedPreset + " " + normalizedSlot + "번 룬 해제");
            return true;
        }

        public static void SaveRuneSlots(
            SaveManager saveManager,
            IReadOnlyDictionary<string, RuneState> runesById,
            int accountLevel,
            int preset,
            IReadOnlyList<string> runeIds)
        {
            if (saveManager == null)
            {
                return;
            }

            int normalizedPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
            var usedRunes = new HashSet<string>();
            for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
            {
                string runeId = runeIds != null && slot - 1 < runeIds.Count ? runeIds[slot - 1] : string.Empty;
                RuneState state = FindRune(runesById, runeId);
                bool valid = IsRuneSlotUnlocked(accountLevel, slot)
                    && state != null
                    && state.Unlocked
                    && !usedRunes.Contains(state.Definition.Id);
                string savedId = valid ? state.Definition.Id : UnequippedRuneId;
                if (valid)
                {
                    usedRunes.Add(state.Definition.Id);
                }

                saveManager.SaveString(SaveKeys.HeroFormationRune(normalizedPreset, slot), savedId);
            }
        }
    }
}
