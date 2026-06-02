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
        public RuneState GetRuneState(string runeId)
        {
            return TotemRuneCommandService.GetRuneState(runesById, runeId);
        }

        public int GetRuneSlotUnlockLevel(int slot)
        {
            return GameData.GetRuneSlotUnlockLevel(slot);
        }

        public bool IsRuneSlotUnlocked(int slot)
        {
            return FormationLoadoutService.IsRuneSlotUnlocked(GetCurrentAccountLevel(), slot);
        }

        public string GetEquippedRuneId(int preset, int slot)
        {
            return FormationLoadoutService.GetEquippedRuneId(saveManager, runesById, preset, slot);
        }

        public bool SetRuneForPreset(int preset, int slot, string runeId)
        {
            if (!IsReady())
            {
                return false;
            }

            bool changed = FormationLoadoutService.TrySetRuneForPreset(
                saveManager,
                runesById,
                GetCurrentAccountLevel(),
                preset,
                slot,
                runeId,
                out FormationLoadoutChangeResult result);
            return ApplyFormationLoadoutChange(changed, result);
        }

        public bool ClearRuneForPreset(int preset, int slot)
        {
            if (!IsReady())
            {
                return false;
            }

            bool changed = FormationLoadoutService.TryClearRuneForPreset(saveManager, preset, slot, out FormationLoadoutChangeResult result);
            return ApplyFormationLoadoutChange(changed, result);
        }

        public bool TryPromoteRune(string runeId)
        {
            if (!IsReady())
            {
                return false;
            }

            bool promoted = TotemRuneCommandService.TryPromoteRune(
                runesById,
                saveManager,
                activeHeroPreset,
                runeId,
                out RuneState state,
                out bool affectsActiveLoadout,
                out string battleLog);
            return ApplyLoggedChange(
                promoted,
                battleLog,
                () =>
                {
                    SaveRuneState(state);
                    if (affectsActiveLoadout)
                    {
                        StartStage(false);
                        NotifyChanged(BattleChangeFlags.Combat | BattleChangeFlags.TotemRune);
                    }
                    else
                    {
                        NotifyChanged(BattleChangeFlags.TotemRune);
                    }
                });
        }

        public bool TryLevelUpRune(string runeId)
        {
            return TryPromoteRune(runeId);
        }

        public int TryPromoteAllRunes()
        {
            if (!IsReady())
            {
                return 0;
            }

            var changedRunes = new List<RuneState>();
            int promotedCount = TotemRuneCommandService.PromoteAllRunes(
                runes,
                runesById,
                saveManager,
                activeHeroPreset,
                changedRunes,
                out bool affectsActiveLoadout,
                out string battleLog);

            SaveRuneStates(changedRunes, true);
            ApplyBattleLog(battleLog);

            if (affectsActiveLoadout)
            {
                StartStage(false);
                NotifyChanged(BattleChangeFlags.Combat | BattleChangeFlags.TotemRune);
            }
            else
            {
                NotifyChanged(BattleChangeFlags.TotemRune);
            }

            return promotedCount;
        }

        public void DebugUnlockAllRunes()
        {
            if (!IsReady())
            {
                return;
            }

            var changedRunes = new List<RuneState>();
            int changedCount = TotemRuneCommandService.DebugUnlockAllRunes(runes, changedRunes, out string battleLog);
            if (changedCount <= 0)
            {
                return;
            }

            SaveRuneStates(changedRunes, true);
            ApplyBattleLog(battleLog);
            NotifyChanged(BattleChangeFlags.TotemRune);
        }

        public void DebugAddRuneItems(int commonRunesPerRune)
        {
            if (!IsReady())
            {
                return;
            }

            var changedRunes = new List<RuneState>();
            int changedCount = TotemRuneCommandService.DebugAddRuneItems(runes, commonRunesPerRune, changedRunes, out string battleLog);
            if (changedCount <= 0)
            {
                return;
            }

            SaveRuneStates(changedRunes, true);
            ApplyBattleLog(battleLog);
            NotifyChanged(BattleChangeFlags.TotemRune);
        }
    }
}
