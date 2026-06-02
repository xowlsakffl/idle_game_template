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
        public TotemState GetTotemState(string totemId)
        {
            return TotemRuneCommandService.GetTotemState(totemsById, totemId);
        }

        public bool TryLevelUpTotem(string totemId)
        {
            if (!IsReady())
            {
                return false;
            }

            bool leveledUp = TotemRuneCommandService.TryLevelUpTotem(
                totemsById,
                totemId,
                wallet,
                out TotemState state,
                out string battleLog);
            return ApplyLoggedChange(
                leveledUp,
                battleLog,
                () =>
                {
                    SaveTotemState(state);
                    StartStage(false);
                    NotifyChanged(BattleChangeFlags.Combat | BattleChangeFlags.TotemRune);
                });
        }

        public bool CanPromoteTotemTier(string totemId)
        {
            return TotemRuneCommandService.CanPromoteTotemTier(totemsById, totems, totemId);
        }

        public bool TryPromoteTotem(string totemId)
        {
            if (!IsReady())
            {
                return false;
            }

            var changedTotems = new List<TotemState>();
            bool promoted = TotemRuneCommandService.TryPromoteTotem(
                totemsById,
                totems,
                totemId,
                wallet,
                changedTotems,
                out string battleLog);
            return ApplyLoggedChange(
                promoted,
                battleLog,
                () =>
                {
                    SaveTotemStates(changedTotems, true);
                    StartStage(false);
                    NotifyChanged(BattleChangeFlags.Combat | BattleChangeFlags.TotemRune);
                });
        }

        public void DebugUnlockAllTotems()
        {
            if (!IsReady())
            {
                return;
            }

            var changedTotems = new List<TotemState>();
            int changedCount = TotemRuneCommandService.DebugUnlockAllTotems(totems, changedTotems, out string battleLog);
            if (changedCount <= 0)
            {
                return;
            }

            SaveTotemStates(changedTotems, true);
            ApplyBattleLog(battleLog);
            NotifyChanged(BattleChangeFlags.TotemRune);
        }

    }
}
