using System;

public static class NumberFormatter
{
    private const double UnitStep = 1000d;
    private static readonly string[] Units =
    {
        "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
        "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
    };

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

        double mantissa = value.Mantissa;
        int unitExponent = value.UnitExponent;
        if (unitExponent <= 0)
        {
            double rawValue = value.ToDoubleClamped();
            double rawAbs = Math.Abs(rawValue);
            return rawValue.ToString(rawValue % 1d == 0d ? "0" : rawAbs >= 100d ? "0.#" : "0.##");
        }

        double abs = Math.Abs(mantissa);
        string numberFormat = abs >= 100d ? "0" : abs >= 10d ? "0.#" : "0.##";
        return mantissa.ToString(numberFormat) + GameNumber.BuildUnitLabel(unitExponent);
    }
}
