using System.Globalization;
using System;

namespace IdleGame.Data
{
    [Serializable]
    public readonly struct GameNumber : IComparable<GameNumber>, IEquatable<GameNumber>
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

        public int CompareTo(GameNumber other)
        {
            if (Mantissa == 0d && other.Mantissa == 0d)
            {
                return 0;
            }

            if (Mantissa >= 0d && other.Mantissa < 0d)
            {
                return 1;
            }

            if (Mantissa < 0d && other.Mantissa >= 0d)
            {
                return -1;
            }

            bool negative = Mantissa < 0d;
            if (UnitExponent != other.UnitExponent)
            {
                int exponentCompare = UnitExponent.CompareTo(other.UnitExponent);
                return negative ? -exponentCompare : exponentCompare;
            }

            return Mantissa.CompareTo(other.Mantissa);
        }

        public bool Equals(GameNumber other)
        {
            return UnitExponent == other.UnitExponent && Math.Abs(Mantissa - other.Mantissa) < 0.000000001d;
        }

        public override bool Equals(object obj)
        {
            return obj is GameNumber other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mantissa.GetHashCode() * 397) ^ UnitExponent;
            }
        }

        public override string ToString()
        {
            return NumberFormatter.Format(this);
        }

        public static GameNumber operator +(GameNumber left, GameNumber right)
        {
            if (left.IsZero)
            {
                return right;
            }

            if (right.IsZero)
            {
                return left;
            }

            int exponentDelta = left.UnitExponent - right.UnitExponent;
            if (exponentDelta >= 18)
            {
                return left;
            }

            if (exponentDelta <= -18)
            {
                return right;
            }

            int targetExponent = Math.Max(left.UnitExponent, right.UnitExponent);
            double leftMantissa = left.Mantissa * Math.Pow(UnitStep, left.UnitExponent - targetExponent);
            double rightMantissa = right.Mantissa * Math.Pow(UnitStep, right.UnitExponent - targetExponent);
            return new GameNumber(leftMantissa + rightMantissa, targetExponent);
        }

        public static GameNumber operator -(GameNumber left, GameNumber right)
        {
            return left + new GameNumber(-right.Mantissa, right.UnitExponent);
        }

        public static GameNumber operator *(GameNumber left, double right)
        {
            return new GameNumber(left.Mantissa * right, left.UnitExponent);
        }

        public static GameNumber operator *(double left, GameNumber right)
        {
            return right * left;
        }

        public static GameNumber operator *(GameNumber left, GameNumber right)
        {
            return new GameNumber(left.Mantissa * right.Mantissa, left.UnitExponent + right.UnitExponent);
        }

        public static GameNumber operator /(GameNumber left, double right)
        {
            return right == 0d ? Zero : new GameNumber(left.Mantissa / right, left.UnitExponent);
        }

        public static bool operator >(GameNumber left, GameNumber right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <(GameNumber left, GameNumber right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >=(GameNumber left, GameNumber right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator <=(GameNumber left, GameNumber right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator ==(GameNumber left, GameNumber right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameNumber left, GameNumber right)
        {
            return !left.Equals(right);
        }

        public static implicit operator GameNumber(double value)
        {
            return FromDouble(value);
        }

        public static implicit operator GameNumber(float value)
        {
            return FromDouble(value);
        }

        public static implicit operator GameNumber(int value)
        {
            return FromDouble(value);
        }

        public static implicit operator GameNumber(long value)
        {
            return FromDouble(value);
        }
    }
}
