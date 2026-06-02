using System;
using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Save;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static partial class BattleStatePersistenceService
    {
        public static void LoadFacilities(
            SaveManager saveManager,
            IList<FacilityState> facilities,
            IDictionary<string, FacilityState> facilitiesById,
            Action<FacilityState> refreshProduction,
            Func<string, HeroState> findHero)
        {
            if (facilities == null || facilitiesById == null)
            {
                return;
            }

            facilities.Clear();
            facilitiesById.Clear();
            long nowTicks = DateTime.UtcNow.Ticks;
            foreach (FacilityDefinition definition in GameData.Facilities)
            {
                FacilityState state = LoadFacilityState(saveManager, definition, nowTicks);
                facilities.Add(state);
                facilitiesById[definition.Id] = state;
                refreshProduction?.Invoke(state);
                SaveFacilityState(saveManager, state, false);
            }

            NormalizeFacilityAssignments(saveManager, facilities, findHero);
            saveManager?.Flush();
        }

        public static void NormalizeFacilityAssignments(
            SaveManager saveManager,
            IEnumerable<FacilityState> facilities,
            Func<string, HeroState> findHero)
        {
            if (facilities == null)
            {
                return;
            }

            var usedHeroIds = new HashSet<string>();
            foreach (FacilityState state in facilities)
            {
                if (state == null)
                {
                    continue;
                }

                NormalizeFacilityAssignmentSlots(state, findHero, usedHeroIds);
                SaveFacilityState(saveManager, state, false);
            }
        }

        public static void SaveFacilityState(SaveManager saveManager, FacilityState state, bool flush)
        {
            if (state == null || saveManager == null)
            {
                return;
            }

            PlayerPrefs.SetInt(SaveKeys.FacilityLevel(state.Definition.Id), state.Level);
            saveManager.SaveGameNumber(SaveKeys.FacilityStoredAmount(state.Definition.Id), state.StoredAmount);
            saveManager.SaveLong(SaveKeys.FacilityLastUpdateUtcTicks(state.Definition.Id), state.LastUpdateUtcTicks);
            for (int slot = 0; slot < FacilityDefinition.MaxAssignedHeroSlots; slot++)
            {
                saveManager.SaveString(SaveKeys.FacilityAssignedHero(state.Definition.Id, slot), state.GetAssignedHeroId(slot));
            }

            if (flush)
            {
                saveManager.Flush();
            }
        }

        public static void SaveFacilityStates(SaveManager saveManager, IEnumerable<FacilityState> states, bool flush)
        {
            if (states == null || saveManager == null)
            {
                return;
            }

            foreach (FacilityState state in states)
            {
                SaveFacilityState(saveManager, state, false);
            }

            if (flush)
            {
                saveManager.Flush();
            }
        }

        private static FacilityState LoadFacilityState(SaveManager saveManager, FacilityDefinition definition, long nowTicks)
        {
            int level = Mathf.Clamp(PlayerPrefs.GetInt(SaveKeys.FacilityLevel(definition.Id), 1), 1, FacilityDefinition.MaxLevel);
            GameNumber storedAmount = saveManager != null
                ? saveManager.LoadGameNumber(SaveKeys.FacilityStoredAmount(definition.Id), GameNumber.Zero)
                : GameNumber.Zero;
            long lastUpdateTicks = saveManager != null
                ? saveManager.LoadLong(SaveKeys.FacilityLastUpdateUtcTicks(definition.Id), nowTicks)
                : nowTicks;
            var state = new FacilityState(definition, level, storedAmount, lastUpdateTicks);
            if (saveManager == null)
            {
                return state;
            }

            for (int slot = 0; slot < FacilityDefinition.MaxAssignedHeroSlots; slot++)
            {
                state.SetAssignedHeroId(slot, saveManager.LoadString(SaveKeys.FacilityAssignedHero(definition.Id, slot), string.Empty));
            }

            return state;
        }

        private static void NormalizeFacilityAssignmentSlots(
            FacilityState state,
            Func<string, HeroState> findHero,
            ISet<string> usedHeroIds)
        {
            for (int slot = 0; slot < FacilityDefinition.MaxAssignedHeroSlots; slot++)
            {
                string heroId = state.GetAssignedHeroId(slot);
                bool unlocked = slot < state.UnlockedSlotCount;
                HeroState hero = !string.IsNullOrEmpty(heroId) ? findHero?.Invoke(heroId) : null;
                if (!unlocked
                    || string.IsNullOrEmpty(heroId)
                    || hero == null
                    || !hero.IsOwned
                    || (usedHeroIds != null && usedHeroIds.Contains(heroId)))
                {
                    state.SetAssignedHeroId(slot, string.Empty);
                    continue;
                }

                usedHeroIds?.Add(heroId);
            }
        }
    }
}
