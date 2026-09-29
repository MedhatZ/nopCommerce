namespace CalculatorBridge.Demo.Calculators;

public interface ICalculatorStrategy
{
    string Key { get; }
    string DisplayName { get; }
    string? Note { get; }
    int CalculateQuantity(decimal area, decimal coveragePerPack, int minimumQuantity);
    string DescribeFormula(decimal area, decimal coveragePerPack, int quantity);
}
