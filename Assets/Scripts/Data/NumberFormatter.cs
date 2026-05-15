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
        if (double.IsNaN(value))
        {
            return "0";
        }

        if (double.IsInfinity(value))
        {
            return value < 0d ? "-999Z" : "999Z";
        }

        double abs = Math.Abs(value);
        if (abs < UnitStep)
        {
            return value.ToString(value % 1d == 0d ? "0" : abs >= 100d ? "0.#" : "0.##");
        }

        int unitIndex = -1;
        while (abs >= UnitStep && unitIndex < Units.Length - 1)
        {
            abs /= UnitStep;
            unitIndex += 1;
        }

        double signedValue = value < 0d ? -abs : abs;
        string numberFormat = abs >= 100d ? "0" : abs >= 10d ? "0.#" : "0.##";
        return signedValue.ToString(numberFormat) + Units[unitIndex];
    }
}
