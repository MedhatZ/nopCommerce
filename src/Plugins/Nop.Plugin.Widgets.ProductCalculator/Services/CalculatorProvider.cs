using Nop.Plugin.Widgets.ProductCalculator.Calculators;

namespace Nop.Plugin.Widgets.ProductCalculator.Services;

public class CalculatorProvider
{
    private readonly IReadOnlyDictionary<CalculatorType, ICalculator> _calculators;

    public CalculatorProvider(IEnumerable<ICalculator> calculators)
    {
        _calculators = calculators.ToDictionary(calculator => calculator.Type);
    }

    public ICalculator Get(CalculatorType type)
    {
        return _calculators.TryGetValue(type, out var calculator)
            ? calculator
            : _calculators[CalculatorType.Flooring];
    }
}
