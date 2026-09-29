namespace Nop.Plugin.Widgets.ProductCalculator.Models;

public class CalculatorRequest
{
    public int ProductId { get; set; }

    public decimal? Length { get; set; }

    public decimal? Width { get; set; }
}
