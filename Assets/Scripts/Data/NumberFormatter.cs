using System;

namespace IdleGame.Data
{
    public static class NumberFormatter
    {
        public static string Format(double value)
        {
            return Format(GameNumber.FromDouble(value));
        }

        public static string Format(GameNumber value)
        {
            if (value.IsZero)
            {
                return "0";
            }

            double rawValue = GameData.ClampVisibleNumber(value.ToDoubleClamped());
            double rawAbs = Math.Abs(rawValue);
            if (rawAbs >= 1000d)
            {
                return rawValue.ToString("#,0");
            }

            return rawValue.ToString(rawValue % 1d == 0d ? "0" : rawAbs >= 100d ? "0.#" : "0.##");
        }
    }
}
