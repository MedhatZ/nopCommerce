using CalculatorBridge.Demo.Calculators;
using CalculatorBridge.Demo.Data;
using CalculatorBridge.Demo.Models;

namespace CalculatorBridge.Demo.Services;

public sealed class ProductService
{
    private readonly DemoStore _store;
    private readonly IReadOnlyDictionary<string, ICalculatorStrategy> _calculators;

    public ProductService(DemoStore store, IEnumerable<ICalculatorStrategy> calculators)
    {
        _store = store;
        _calculators = calculators.ToDictionary(calculator => calculator.Key, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<CalculatorListItem> GetCalculators() =>
        _calculators.Values
            .OrderBy(calculator => calculator.DisplayName)
            .Select(calculator => new CalculatorListItem(calculator.Key, calculator.DisplayName, calculator.Note))
            .ToList();

    public IReadOnlyList<ProductListItem> GetProducts() =>
        _store.GetProducts()
            .Select(product => new ProductListItem(product.Id, product.Name, product.Type, product.StatusLabel))
            .ToList();

    public ProductDetail? GetProduct(int id)
    {
        var product = _store.GetProduct(id);
        return product is null ? null : ToDetail(product);
    }

    public (ProductDetail? Product, string? Error) SaveConfiguration(int id, ProductConfigurationRequest request)
    {
        var existing = _store.GetProduct(id);
        if (existing is null)
            return (null, "Product not found.");

        if (string.Equals(existing.Type, "Accessory", StringComparison.OrdinalIgnoreCase))
            return (null, "Linked accessories do not have their own calculator configuration.");

        if (string.IsNullOrWhiteSpace(request.CalculatorKey) || !_calculators.ContainsKey(request.CalculatorKey))
            return (null, "Choose a registered calculator.");

        if (request.CoveragePerPack <= 0)
            return (null, "Coverage per pack must be greater than zero.");

        if (request.MinimumQuantity < 1)
            return (null, "Minimum quantity must be at least 1.");

        if (request.LinkedProductId is int linkedId)
        {
            var linked = _store.GetProduct(linkedId);
            if (linked is null || !string.Equals(linked.Type, "Accessory", StringComparison.OrdinalIgnoreCase))
                return (null, "Linked product must be an accessory.");

            if (request.LinkedQuantity < 1 || request.LinkedPerQuantity < 1)
                return (null, "Quantity rule values must be at least 1.");
        }

        var updated = _store.UpdateProduct(id, product =>
        {
            product.CalculatorKey = request.CalculatorKey;
            product.CoveragePerPack = request.CoveragePerPack;
            product.MinimumQuantity = request.MinimumQuantity;
            product.CalculatorEnabled = request.EnableCalculator;
            product.LinkedProductId = request.LinkedProductId;
            product.LinkedQuantity = request.LinkedProductId is null ? 1 : request.LinkedQuantity;
            product.LinkedPerQuantity = request.LinkedProductId is null ? 10 : request.LinkedPerQuantity;
        });

        if (!updated)
            return (null, "Product not found.");

        return (GetProduct(id), null);
    }

    private ProductDetail ToDetail(Product product)
    {
        Product? linked = product.LinkedProductId is int linkedId ? _store.GetProduct(linkedId) : null;
        _calculators.TryGetValue(product.CalculatorKey ?? "", out var calculator);

        return new ProductDetail(
            product.Id,
            product.Name,
            string.IsNullOrWhiteSpace(product.CartName) ? product.Name : product.CartName,
            product.Type,
            product.StatusLabel,
            product.Price,
            product.Unit,
            product.CalculatorKey,
            calculator?.DisplayName,
            calculator?.Note,
            product.CoveragePerPack,
            product.MinimumQuantity,
            product.CalculatorEnabled,
            product.LinkedProductId,
            linked?.Name,
            linked?.Price,
            linked?.Unit,
            product.LinkedQuantity,
            product.LinkedPerQuantity,
            linked is null
                ? null
                : $"{product.LinkedQuantity} {Plural(linked.Unit, product.LinkedQuantity)} per {product.LinkedPerQuantity} {Plural(product.Unit, product.LinkedPerQuantity)}");
    }

    private static string Plural(string unit, int count) => count == 1 ? unit : unit + "s";
}

public sealed record CalculatorListItem(string Key, string DisplayName, string? Note);

public sealed record ProductListItem(int Id, string Name, string Type, string Calculator);

public sealed record ProductDetail(
    int Id,
    string Name,
    string CartName,
    string Type,
    string Calculator,
    decimal Price,
    string Unit,
    string? CalculatorKey,
    string? CalculatorName,
    string? Note,
    decimal CoveragePerPack,
    int MinimumQuantity,
    bool EnableCalculator,
    int? LinkedProductId,
    string? LinkedProductName,
    decimal? LinkedProductPrice,
    string? LinkedProductUnit,
    int LinkedQuantity,
    int LinkedPerQuantity,
    string? QuantityRule);
