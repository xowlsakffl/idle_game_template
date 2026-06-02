using System;

namespace IdleGame.Data
{
    public readonly partial struct GameNumber
    {
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
