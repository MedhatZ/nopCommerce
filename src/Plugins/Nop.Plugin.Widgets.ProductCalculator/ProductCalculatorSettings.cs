using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.ProductCalculator;

public class ProductCalculatorSettings : ISettings
{
    public bool EnableCalculator { get; set; } = true;

    public CalculatorType CalculatorType { get; set; } = CalculatorType.Flooring;

    public decimal Coverage { get; set; } = 2.5m;

    public int LinkedProductId { get; set; }
}

public enum CalculatorType
{
    Flooring = 0,
    WallPanel = 10
}
