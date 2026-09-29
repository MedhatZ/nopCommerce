namespace Nop.Plugin.Widgets.ProductCalculator.Calculators;

public class WallPanelCalculator : ICalculator
{
    public CalculatorType Type => CalculatorType.WallPanel;

    public PackCalculation Calculate(decimal roomLength, decimal roomWidth, decimal coverage)
    {
        if (roomLength <= 0 || roomWidth <= 0 || coverage <= 0)
            return PackCalculation.Invalid(ProductCalculatorDefaults.InvalidDimensions);

        var panels = (int)Math.Ceiling((roomLength * roomWidth) / coverage);
        return PackCalculation.Ok(panels);
    }
}
