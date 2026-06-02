using System.Globalization;
using System;

namespace IdleGame.Data
{
    [Serializable]
    public readonly partial struct GameNumber : IComparable<GameNumber>, IEquatable<GameNumber>
    {
        private const double UnitStep = 1000d;
        private const double LogUnitStep = 6.907755278982137d;

        public static readonly GameNumber Zero = new GameNumber(0d, 0);
        public static readonly GameNumber One = new GameNumber(1d, 0);

        public GameNumber(double mantissa, int unitExponent)
        {
            if (double.IsNaN(mantissa) || mantissa == 0d)
            {
                Mantissa = 0d;
                UnitExponent = 0;
                return;
            }

            if (double.IsInfinity(mantissa))
            {
                Mantissa = mantissa > 0d ? 999.999d : -999.999d;
                UnitExponent = 1000000000;
                return;
            }

            double normalizedMantissa = mantissa;
            int normalizedExponent = unitExponent;
            double abs = Math.Abs(normalizedMantissa);
            if (abs >= UnitStep || abs < 1d)
            {
                int shift = (int)Math.Floor(Math.Log(abs) / LogUnitStep);
                normalizedMantissa /= Math.Pow(UnitStep, shift);
                normalizedExponent += shift;
            }

            abs = Math.Abs(normalizedMantissa);
            while (abs >= UnitStep)
            {
                normalizedMantissa /= UnitStep;
                normalizedExponent += 1;
                abs = Math.Abs(normalizedMantissa);
            }

            while (abs > 0d && abs < 1d)
            {
                normalizedMantissa *= UnitStep;
                normalizedExponent -= 1;
                abs = Math.Abs(normalizedMantissa);
            }

            if (normalizedExponent < 0 && abs < UnitStep)
            {
                double value = normalizedMantissa * Math.Pow(UnitStep, normalizedExponent);
                Mantissa = Math.Abs(value) < 0.000000001d ? 0d : value;
                UnitExponent = 0;
                return;
            }

            Mantissa = normalizedMantissa;
            UnitExponent = normalizedExponent;
        }

        public double Mantissa { get; }
        public int UnitExponent { get; }
        public bool IsZero => Mantissa == 0d;

        public static GameNumber FromDouble(double value)
        {
            return new GameNumber(value, 0);
        }

        public static GameNumber FromGrowth(double baseValue, double growth, double steps)
        {
            if (double.IsNaN(baseValue) || double.IsNaN(growth) || baseValue <= 0d || growth <= 0d)
            {
                return Zero;
            }

            double logValue = Math.Log(baseValue) + Math.Max(0d, steps) * Math.Log(growth);
            if (double.IsNaN(logValue) || double.IsNegativeInfinity(logValue))
            {
                return Zero;
            }

            if (double.IsPositiveInfinity(logValue))
            {
                return new GameNumber(999.999d, 1000000000);
            }

            int unitExponent = (int)Math.Floor(logValue / LogUnitStep);
            double mantissa = Math.Exp(logValue - unitExponent * LogUnitStep);
            return new GameNumber(mantissa, unitExponent);
        }

        public static GameNumber Floor(GameNumber value)
        {
            if (value.UnitExponent > 0)
            {
                return value;
            }

            return new GameNumber(Math.Floor(value.Mantissa), 0);
        }

        public static GameNumber Ceiling(GameNumber value)
        {
            if (value.UnitExponent > 0)
            {
                return value;
            }

            return new GameNumber(Math.Ceiling(value.Mantissa), 0);
        }

        public static GameNumber Max(GameNumber left, GameNumber right)
        {
            return left.CompareTo(right) >= 0 ? left : right;
        }

        public static GameNumber Min(GameNumber left, GameNumber right)
        {
            return left.CompareTo(right) <= 0 ? left : right;
        }

        public double RatioTo(GameNumber denominator)
        {
            if (denominator.Mantissa <= 0d)
            {
                return 0d;
            }

            if (Mantissa <= 0d)
            {
                return 0d;
            }

            int exponentDelta = UnitExponent - denominator.UnitExponent;
            if (exponentDelta > 100)
            {
                return double.PositiveInfinity;
            }

            if (exponentDelta < -100)
            {
                return 0d;
            }

            return (Mantissa / denominator.Mantissa) * Math.Pow(UnitStep, exponentDelta);
        }

        public double ToDoubleClamped()
        {
            if (Mantissa == 0d)
            {
                return 0d;
            }

            double logValue = Math.Log(Math.Abs(Mantissa)) + UnitExponent * LogUnitStep;
            if (logValue >= Math.Log(double.MaxValue / 1024d))
            {
                return Mantissa < 0d ? -double.MaxValue / 1024d : double.MaxValue / 1024d;
            }

            double value = Math.Exp(logValue);
            return Mantissa < 0d ? -value : value;
        }

        public string ToSaveString()
        {
            return Mantissa.ToString("R", CultureInfo.InvariantCulture)
                + "|"
                + UnitExponent.ToString(CultureInfo.InvariantCulture);
        }

        public static bool TryParse(string raw, out GameNumber value)
        {
            value = Zero;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            string[] parts = raw.Split('|');
            if (parts.Length == 2
                && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double mantissa)
                && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int exponent))
            {
                value = new GameNumber(mantissa, exponent);
                return true;
            }

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double legacyDouble))
            {
                value = FromDouble(legacyDouble);
                return true;
            }

            return false;
        }


    }
}
