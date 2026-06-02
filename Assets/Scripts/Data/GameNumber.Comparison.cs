using System;

namespace IdleGame.Data
{
    public readonly partial struct GameNumber
    {
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
    }
}
