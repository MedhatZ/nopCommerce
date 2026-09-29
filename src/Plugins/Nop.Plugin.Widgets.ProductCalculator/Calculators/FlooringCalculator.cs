namespace Nop.Plugin.Widgets.ProductCalculator.Calculators;

public class FlooringCalculator : ICalculator
{
    public CalculatorType Type => CalculatorType.Flooring;

    public PackCalculation Calculate(decimal roomLength, decimal roomWidth, decimal coverage)
    {
        if (roomLength <= 0 || roomWidth <= 0 || coverage <= 0)
            return PackCalculation.Invalid(ProductCalculatorDefaults.InvalidDimensions);

        var packs = (int)Math.Ceiling((roomLength * roomWidth) / coverage);
        return PackCalculation.Ok(packs);
    }
}
