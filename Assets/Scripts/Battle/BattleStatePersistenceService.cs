using System;
using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Save;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static partial class BattleStatePersistenceService
    {
        internal readonly struct FortressSaveState
        {
            public FortressSaveState(int level, GameNumber experience)
            {
                Level = level;
                Experience = experience;
            }

            public int Level { get; }
            public GameNumber Experience { get; }
        }

        public static FortressSaveState LoadFortress(SaveManager saveManager, int maxLevel)
        {
            int level = Mathf.Clamp(PlayerPrefs.GetInt(SaveKeys.FortressLevel, 1), 1, maxLevel);
            GameNumber experience = saveManager != null
                ? saveManager.LoadGameNumber(SaveKeys.FortressExperience, GameNumber.Zero)
                : GameNumber.Zero;
            return new FortressSaveState(level, experience);
        }

        public static void SaveFortress(SaveManager saveManager, int level, GameNumber experience, bool flush)
        {
            if (saveManager == null)
            {
                return;
            }

            PlayerPrefs.SetInt(SaveKeys.FortressLevel, level);
            saveManager.SaveGameNumber(SaveKeys.FortressExperience, experience);
            if (flush)
            {
                saveManager.Flush();
            }
        }
    }
}
