using System;
using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Save;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static partial class BattleStatePersistenceService
    {
        public static void LoadTotems(
            SaveManager saveManager,
            IList<TotemState> totems,
            IDictionary<string, TotemState> totemsById)
        {
            if (totems == null || totemsById == null)
            {
                return;
            }

            totems.Clear();
            totemsById.Clear();
            foreach (TotemDefinition definition in GameData.Totems)
            {
                int level = Mathf.Clamp(PlayerPrefs.GetInt(SaveKeys.TotemLevel(definition.Id), 1), 1, TotemDefinition.MaxLevel);
                TotemGrade grade = (TotemGrade)Mathf.Clamp(PlayerPrefs.GetInt(SaveKeys.TotemGrade(definition.Id), 0), 0, (int)TotemGrade.Mythic);
                bool unlocked = saveManager != null
                    ? saveManager.LoadBool(SaveKeys.TotemUnlocked(definition.Id), definition.StartUnlocked)
                    : definition.StartUnlocked;
                var state = new TotemState(definition, level, grade, unlocked);
                totems.Add(state);
                totemsById[definition.Id] = state;
            }
        }

        public static void SaveTotemState(SaveManager saveManager, TotemState state, bool flush)
        {
            if (state == null || saveManager == null)
            {
                return;
            }

            PlayerPrefs.SetInt(SaveKeys.TotemLevel(state.Definition.Id), state.Level);
            PlayerPrefs.SetInt(SaveKeys.TotemGrade(state.Definition.Id), (int)state.Grade);
            saveManager.SaveBool(SaveKeys.TotemUnlocked(state.Definition.Id), state.Unlocked);
            if (flush)
            {
                saveManager.Flush();
            }
        }

        public static void SaveTotemStates(SaveManager saveManager, IEnumerable<TotemState> states, bool flush)
        {
            if (states == null || saveManager == null)
            {
                return;
            }

            foreach (TotemState state in states)
            {
                SaveTotemState(saveManager, state, false);
            }

            if (flush)
            {
                saveManager.Flush();
            }
        }
    }
}
