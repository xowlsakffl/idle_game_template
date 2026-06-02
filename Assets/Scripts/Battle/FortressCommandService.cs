using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static class FortressCommandService
    {
        public static bool TryLevelUp(
            int currentLevel,
            GameNumber experience,
            int maxLevel,
            out int nextLevel,
            out string battleLog)
        {
            nextLevel = currentLevel;
            if (currentLevel >= maxLevel)
            {
                battleLog = "요새는 이미 최고 레벨입니다.";
                return false;
            }

            GameNumber requiredExperience = FortressCombatService.GetRequiredExperienceForLevel(currentLevel + 1, maxLevel);
            if (experience < requiredExperience)
            {
                battleLog = "요새 경험치가 부족합니다.";
                return false;
            }

            nextLevel = Mathf.Min(maxLevel, currentLevel + 1);
            battleLog = "요새 Lv." + nextLevel + " 강화 완료";
            return true;
        }

        public static bool TryDebugLevel(
            int currentLevel,
            int levels,
            int maxLevel,
            out int nextLevel,
            out string battleLog)
        {
            nextLevel = currentLevel;
            battleLog = string.Empty;
            if (levels <= 0)
            {
                return false;
            }

            nextLevel = Mathf.Clamp(currentLevel + levels, 1, maxLevel);
            battleLog = "요새 Lv." + nextLevel;
            return true;
        }

        public static bool TryDebugAddExperience(GameNumber amount, out string battleLog)
        {
            if (amount <= GameNumber.Zero)
            {
                battleLog = string.Empty;
                return false;
            }

            battleLog = "요새 EXP +" + NumberFormatter.Format(amount);
            return true;
        }
    }
}
