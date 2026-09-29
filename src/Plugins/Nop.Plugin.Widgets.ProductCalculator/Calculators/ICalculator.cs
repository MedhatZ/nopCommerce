namespace Nop.Plugin.Widgets.ProductCalculator.Calculators;

public interface ICalculator
{
    CalculatorType Type { get; }

    PackCalculation Calculate(decimal roomLength, decimal roomWidth, decimal coverage);
}

public readonly record struct PackCalculation(bool IsValid, int Packs, string Error)
{
    public static PackCalculation Invalid(string error) => new(false, 0, error);

    public static PackCalculation Ok(int packs) => new(true, packs, null);
}
