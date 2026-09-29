using System.Globalization;

namespace CalculatorBridge.Demo.Calculators;

public sealed class WallPanelCalculatorStrategy : ICalculatorStrategy
{
    private const decimal CuttingAllowance = 1.10m;

    public string Key => "wall-panel";
    public string DisplayName => "Wall Panel Calculator";
    public string? Note => "Includes a 10% cutting allowance.";

    public int CalculateQuantity(decimal area, decimal coveragePerPack, int minimumQuantity)
    {
        var adjustedArea = area * CuttingAllowance;
        var panels = (int)Math.Ceiling(adjustedArea / coveragePerPack);
        return Math.Max(panels, Math.Max(minimumQuantity, 1));
    }

    public string DescribeFormula(decimal area, decimal coveragePerPack, int quantity) =>
        $"Ceiling(({Format(area)} x 1.10) / {Format(coveragePerPack)}) = {quantity}";

    private static string Format(decimal value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);
}
