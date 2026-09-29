using System.Globalization;

namespace CalculatorBridge.Demo.Calculators;

public sealed class FlooringCalculatorStrategy : ICalculatorStrategy
{
    public string Key => "flooring";
    public string DisplayName => "Flooring Calculator";
    public string? Note => null;

    public int CalculateQuantity(decimal area, decimal coveragePerPack, int minimumQuantity)
    {
        var packs = (int)Math.Ceiling(area / coveragePerPack);
        return Math.Max(packs, Math.Max(minimumQuantity, 1));
    }

    public string DescribeFormula(decimal area, decimal coveragePerPack, int quantity) =>
        $"Ceiling({Format(area)} / {Format(coveragePerPack)}) = {quantity}";

    private static string Format(decimal value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);
}
