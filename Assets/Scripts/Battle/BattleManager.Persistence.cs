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
        private void LoadTotems()
        {
            BattleStatePersistenceService.LoadTotems(saveManager, totems, totemsById);
        }

        private void LoadRunes()
        {
            BattleStatePersistenceService.LoadRunes(saveManager, runes, runesById);
        }

        private void LoadFacilities()
        {
            BattleStatePersistenceService.LoadFacilities(
                saveManager,
                facilities,
                facilitiesById,
                state => RefreshFacilityProduction(state, false),
                FindHero);
        }

        private void SaveFacilityState(FacilityState state, bool flush)
        {
            BattleStatePersistenceService.SaveFacilityState(saveManager, state, flush);
        }

        private void SaveFacilityStates(IEnumerable<FacilityState> states, bool flush)
        {
            BattleStatePersistenceService.SaveFacilityStates(saveManager, states, flush);
        }

        private void SaveRuneState(RuneState state, bool flush = true)
        {
            BattleStatePersistenceService.SaveRuneState(saveManager, state, flush);
        }

        private void SaveTotemState(TotemState state, bool flush = true)
        {
            BattleStatePersistenceService.SaveTotemState(saveManager, state, flush);
        }

        private void SaveRuneStates(IEnumerable<RuneState> states, bool flush)
        {
            BattleStatePersistenceService.SaveRuneStates(saveManager, states, flush);
        }

        private void SaveTotemStates(IEnumerable<TotemState> states, bool flush)
        {
            BattleStatePersistenceService.SaveTotemStates(saveManager, states, flush);
        }

        private void SaveHeroState(HeroState hero, bool flush)
        {
            if (hero == null || saveManager == null)
            {
                return;
            }

            saveManager.SaveHero(hero);
            if (flush)
            {
                saveManager.Flush();
            }
        }

        private void SaveHeroStates(IEnumerable<HeroState> states, bool flush)
        {
            if (states != null)
            {
                foreach (HeroState hero in states)
                {
                    SaveHeroState(hero, false);
                }
            }

            if (flush)
            {
                saveManager.Flush();
            }
        }

        private void SaveHeroTranscendOption(HeroState hero, int slotIndex, bool flush)
        {
            if (hero == null || saveManager == null)
            {
                return;
            }

            saveManager.SaveHeroTranscendOption(hero, slotIndex);
            if (flush)
            {
                saveManager.Flush();
            }
        }
    }
}
