using System;
using IdleGame.Data;
using IdleGame.Progression;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static class FortressCombatService
    {
        public static GameNumber GetRequiredExperienceForLevel(int level, int maxLevel)
        {
            if (level <= 1)
            {
                return GameNumber.Zero;
            }

            int normalizedLevel = Mathf.Clamp(level, 1, maxLevel);
            double steps = normalizedLevel - 1;
            double required = 62d * Math.Pow(steps, 2.08d) + 35d * steps;
            return GameData.ClampNumber(GameNumber.Ceiling(GameNumber.FromDouble(required)));
        }

        public static GameNumber CalculateMaxHp(int level, int maxLevel)
        {
            int normalizedLevel = Mathf.Clamp(level, 1, maxLevel);
            double hp = 520d + normalizedLevel * 38d + Math.Pow(normalizedLevel, 1.22d) * 16d;
            return GameData.ClampNumber(GameNumber.Floor(GameNumber.FromDouble(hp)));
        }

        public static GameNumber CalculateAttackPower(int level, int maxLevel)
        {
            int normalizedLevel = Mathf.Clamp(level, 1, maxLevel);
            double attack = 12d + normalizedLevel * 2.6d + Math.Pow(normalizedLevel, 1.16d) * 1.8d;
            return NormalizeDamage(attack);
        }

        public static float CalculateAttackInterval(int level, int maxLevel)
        {
            return Mathf.Max(0.82f, 2.10f - Mathf.Clamp(level, 1, maxLevel) * 0.0065f);
        }

        public static float CalculateAttackRange(int level, int maxLevel)
        {
            return Mathf.Min(4.35f, 2.35f + Mathf.Clamp(level, 1, maxLevel) * 0.018f);
        }

        public static double CalculateCombatPower(int level, int maxLevel)
        {
            int normalizedLevel = Mathf.Clamp(level, 1, maxLevel);
            double power = 300d + normalizedLevel * 18d + Math.Pow(normalizedLevel, 1.08d) * 2.6d;
            return Math.Max(1d, Math.Floor(power));
        }

        public static GameNumber GetExperienceReward(StageDefinition stage, bool boss)
        {
            if (stage == null)
            {
                return GameNumber.Zero;
            }

            int stageIndex = GameData.GetStageIndex(stage.Id) + 1;
            double reward = 3d + stageIndex * 0.85d;
            if (boss)
            {
                reward *= 24d;
            }

            return GameNumber.Floor(GameNumber.FromDouble(Math.Max(1d, reward)));
        }

        private static GameNumber NormalizeDamage(double damage)
        {
            if (double.IsNaN(damage) || damage <= 1d)
            {
                return GameNumber.One;
            }

            if (double.IsInfinity(damage))
            {
                return GameNumber.FromDouble(double.MaxValue / 1024d);
            }

            return GameData.ClampNumber(GameNumber.Floor(GameNumber.Max(GameNumber.One, damage)));
        }
    }
}
