using CalculatorBridge.Demo.Calculators;
using CalculatorBridge.Demo.Data;
using CalculatorBridge.Demo.Models;

namespace CalculatorBridge.Demo.Services;

public sealed class CalculatorService
{
    public const string InvalidDimensions = "Invalid dimensions";
    public const string InvalidDimensionsDetail = "Please enter valid values";

    private readonly DemoStore _store;
    private readonly IReadOnlyDictionary<string, ICalculatorStrategy> _calculators;
    private readonly ILogger<CalculatorService> _logger;

    public CalculatorService(
        DemoStore store,
        IEnumerable<ICalculatorStrategy> calculators,
        ILogger<CalculatorService> logger)
    {
        _store = store;
        _calculators = calculators.ToDictionary(calculator => calculator.Key, StringComparer.OrdinalIgnoreCase);
        _logger = logger;
    }

    public CalculationOutcome Calculate(AddToCartRequest request)
    {
        if (request.Length is not > 0 || request.Width is not > 0)
            return CalculationOutcome.Fail(StatusCodes.Status400BadRequest, InvalidDimensions, InvalidDimensionsDetail);

        var settings = _store.GetSettings();
        if (!settings.EnableCalculator)
            return CalculationOutcome.Fail(StatusCodes.Status409Conflict, "Calculator is turned off", "Enable it from Settings.");

        var storeId = string.IsNullOrWhiteSpace(request.StoreId) ? "uk" : request.StoreId.Trim();
        var store = settings.Stores.FirstOrDefault(item => string.Equals(item.Id, storeId, StringComparison.OrdinalIgnoreCase));
        if (store is null)
            return CalculationOutcome.Fail(StatusCodes.Status400BadRequest, "Unknown store", "Choose a store from Settings.");

        if (!store.Enabled)
            return CalculationOutcome.Fail(StatusCodes.Status409Conflict, "Calculator is disabled for this store", "Enable the store from Settings.");

        var product = _store.GetProduct(request.ProductId);
        if (product is null)
            return CalculationOutcome.Fail(StatusCodes.Status404NotFound, "Product not found", "Use a product id from the dashboard.");

        if (string.IsNullOrWhiteSpace(product.CalculatorKey) || !product.CalculatorEnabled)
            return CalculationOutcome.Fail(StatusCodes.Status409Conflict, "Calculator is disabled for this product", "Enable it from Product Configuration.");

        if (!_calculators.TryGetValue(product.CalculatorKey, out var strategy))
            return CalculationOutcome.Fail(StatusCodes.Status400BadRequest, "Unknown calculator", "This calculator is not registered.");

        if (product.CoveragePerPack <= 0)
            return CalculationOutcome.Fail(StatusCodes.Status400BadRequest, "Invalid coverage", "Set a coverage greater than zero in Product Configuration.");

        var length = request.Length.Value;
        var width = request.Width.Value;
        var area = length * width;
        var quantity = strategy.CalculateQuantity(area, product.CoveragePerPack, product.MinimumQuantity);

        var lines = new List<CartLine> { ToLine(product, quantity) };
        if (product.LinkedProductId is int linkedId && product.LinkedPerQuantity > 0 && product.LinkedQuantity > 0)
        {
            var linked = _store.GetProduct(linkedId);
            if (linked is not null)
            {
                var batches = (int)Math.Ceiling(quantity / (decimal)product.LinkedPerQuantity);
                var linkedQuantity = batches * product.LinkedQuantity;
                if (linkedQuantity > 0)
                    lines.Add(ToLine(linked, linkedQuantity));
            }
        }

        _logger.LogInformation(
            "Calculated {Calculator} for product {ProductId}. Area {Area}, coverage {Coverage}, quantity {Quantity}. Posted quantity {PostedQuantity} was ignored.",
            strategy.DisplayName,
            product.Id,
            area,
            product.CoveragePerPack,
            quantity,
            request.Quantity);

        return CalculationOutcome.Ok(new CalculationSuccess
        {
            Area = area,
            Length = length,
            Width = width,
            CoveragePerPack = product.CoveragePerPack,
            Quantity = quantity,
            Formula = strategy.DescribeFormula(area, product.CoveragePerPack, quantity),
            Calculator = strategy.DisplayName,
            ClientQuantityIgnored = true,
            DiscardedClientInput = request.Quantity is null && request.Coverage is null
                ? null
                : new DiscardedClientInput
                {
                    Quantity = request.Quantity,
                    Coverage = request.Coverage
                },
            Lines = lines,
            Total = lines.Sum(line => line.LineTotal)
        });
    }

    private static CartLine ToLine(Product product, int quantity) => new()
    {
        ProductId = product.Id,
        Name = string.IsNullOrWhiteSpace(product.CartName) ? product.Name : product.CartName,
        Quantity = quantity,
        UnitPrice = product.Price,
        LineTotal = product.Price * quantity
    };
}
