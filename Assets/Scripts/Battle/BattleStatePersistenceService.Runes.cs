using System;
using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Save;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static partial class BattleStatePersistenceService
    {
        public static void LoadRunes(
            SaveManager saveManager,
            IList<RuneState> runes,
            IDictionary<string, RuneState> runesById)
        {
            if (runes == null || runesById == null)
            {
                return;
            }

            runes.Clear();
            runesById.Clear();
            foreach (RuneDefinition definition in GameData.Runes)
            {
                RuneState state = LoadRuneState(saveManager, definition);
                runes.Add(state);
                runesById[definition.Id] = state;
                SaveRuneState(saveManager, state, false);
            }

            saveManager?.Flush();
        }

        public static void SaveRuneState(SaveManager saveManager, RuneState state, bool flush)
        {
            if (state == null || saveManager == null)
            {
                return;
            }

            PlayerPrefs.SetInt(SaveKeys.RuneGrade(state.Definition.Id), (int)state.Grade);
            for (int i = 0; i < RuneState.GradeCount; i++)
            {
                RuneGrade grade = (RuneGrade)i;
                PlayerPrefs.SetInt(SaveKeys.RuneCount(state.Definition.Id, grade), state.GetCount(grade));
            }

            saveManager.SaveBool(SaveKeys.RuneUnlocked(state.Definition.Id), state.Unlocked);
            if (flush)
            {
                saveManager.Flush();
            }
        }

        public static void SaveRuneStates(SaveManager saveManager, IEnumerable<RuneState> states, bool flush)
        {
            if (states == null || saveManager == null)
            {
                return;
            }

            foreach (RuneState state in states)
            {
                SaveRuneState(saveManager, state, false);
            }

            if (flush)
            {
                saveManager.Flush();
            }
        }
        private static RuneState LoadRuneState(SaveManager saveManager, RuneDefinition definition)
        {
            int savedGrade = PlayerPrefs.GetInt(SaveKeys.RuneGrade(definition.Id), -1);
            if (savedGrade < 0)
            {
                int legacyLevel = Mathf.Clamp(PlayerPrefs.GetInt(SaveKeys.RuneLevel(definition.Id), 1), 1, RuneDefinition.MaxLevel);
                savedGrade = Mathf.Clamp((legacyLevel - 1) / 10, 0, (int)RuneDefinition.MaxGrade);
            }

            RuneGrade grade = (RuneGrade)Mathf.Clamp(savedGrade, 0, (int)RuneDefinition.MaxGrade);
            int[] counts = LoadRuneCounts(definition, grade);
            bool unlocked = saveManager != null
                ? saveManager.LoadBool(SaveKeys.RuneUnlocked(definition.Id), definition.StartUnlocked)
                : definition.StartUnlocked;
            return new RuneState(definition, grade, counts, unlocked);
        }

        private static int[] LoadRuneCounts(RuneDefinition definition, RuneGrade grade)
        {
            int[] counts = new int[RuneState.GradeCount];
            bool hasGradeCountSave = false;
            for (int i = 0; i < RuneState.GradeCount; i++)
            {
                RuneGrade countGrade = (RuneGrade)i;
                string countKey = SaveKeys.RuneCount(definition.Id, countGrade);
                if (PlayerPrefs.HasKey(countKey))
                {
                    hasGradeCountSave = true;
                    counts[i] = Mathf.Max(0, PlayerPrefs.GetInt(countKey, 0));
                }
            }

            if (!hasGradeCountSave)
            {
                int gradeIndex = Mathf.Clamp((int)grade, 0, RuneState.GradeCount - 1);
                int legacyCopies = Mathf.Max(0, PlayerPrefs.GetInt(SaveKeys.RuneCopies(definition.Id), 0));
                counts[gradeIndex] = Mathf.Max(definition.StartUnlocked ? 1 : 0, counts[gradeIndex]) + legacyCopies;
            }

            return counts;
        }
    }
}
