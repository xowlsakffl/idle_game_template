using System.Collections.Generic;
using System;
using UnityEngine;

namespace IdleGame.Data
{
    public static partial class GameData
    {
        public static double ClampVisibleNumber(double value)
        {
            if (double.IsNaN(value))
            {
                return 0d;
            }

            if (double.IsPositiveInfinity(value))
            {
                return double.MaxValue / 1024d;
            }

            if (double.IsNegativeInfinity(value))
            {
                return -double.MaxValue / 1024d;
            }

            return value;
        }

        public static GameNumber ClampNumber(GameNumber value)
        {
            if (double.IsNaN(value.Mantissa))
            {
                return GameNumber.Zero;
            }

            return value;
        }

        public static GameNumber ClampNumber(double value)
        {
            return GameNumber.FromDouble(ClampVisibleNumber(value));
        }

        public static double ClampCombatPower(double value)
        {
            if (double.IsNaN(value) || value <= 1d)
            {
                return 1d;
            }

            return ClampVisibleNumber(value);
        }

        public static long ClampCount(long value)
        {
            return Math.Max(0L, value);
        }
    }
}
