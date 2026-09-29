using CalculatorBridge.Demo.Models;

namespace CalculatorBridge.Demo.Data;

/// <summary>
/// Stand-in for nopCommerce repositories. The plugin would use IRepository and ISettingService instead of this class.
/// </summary>
public sealed class DemoStore
{
    private readonly object _gate = new();
    private List<Product> _products = SeedProducts();
    private DemoSettings _settings = SeedSettings();
    private readonly Dictionary<string, List<CartLine>> _carts = new(StringComparer.Ordinal);

    public IReadOnlyList<Product> GetProducts()
    {
        lock (_gate)
            return _products.Select(CopyProduct).ToList();
    }

    public Product? GetProduct(int id)
    {
        lock (_gate)
        {
            var product = _products.FirstOrDefault(item => item.Id == id);
            return product is null ? null : CopyProduct(product);
        }
    }

    public bool UpdateProduct(int id, Action<Product> update)
    {
        lock (_gate)
        {
            var product = _products.FirstOrDefault(item => item.Id == id);
            if (product is null)
                return false;

            update(product);
            return true;
        }
    }

    public DemoSettings GetSettings()
    {
        lock (_gate)
            return CopySettings(_settings);
    }

    public void SaveSettings(DemoSettings settings)
    {
        lock (_gate)
            _settings = CopySettings(settings);
    }

    public IReadOnlyList<CartLine> GetCart(string token)
    {
        lock (_gate)
        {
            if (!_carts.TryGetValue(token, out var lines))
                return [];

            return lines.Select(CopyLine).ToList();
        }
    }

    public void SetCart(string token, IReadOnlyList<CartLine> lines)
    {
        lock (_gate)
            _carts[token] = lines.Select(CopyLine).ToList();
    }

    public void Reset()
    {
        lock (_gate)
        {
            _products = SeedProducts();
            _settings = SeedSettings();
            _carts.Clear();
        }
    }

    private static List<Product> SeedProducts() =>
    [
        new Product
        {
            Id = 100,
            Name = "Oak Flooring Pack",
            CartName = "Oak Flooring Pack",
            Type = "Flooring",
            Price = 45m,
            Unit = "pack",
            CalculatorKey = "flooring",
            CoveragePerPack = 2.5m,
            MinimumQuantity = 1,
            CalculatorEnabled = true,
            LinkedProductId = 102,
            LinkedQuantity = 1,
            LinkedPerQuantity = 10
        },
        new Product
        {
            Id = 101,
            Name = "Wall Panel",
            CartName = "Wall Panel",
            Type = "Wall Panel",
            Price = 38m,
            Unit = "panel",
            CalculatorKey = "wall-panel",
            CoveragePerPack = 2m,
            MinimumQuantity = 1,
            CalculatorEnabled = true
        },
        new Product
        {
            Id = 102,
            Name = "Adhesive",
            CartName = "Adhesive Bucket",
            Type = "Accessory",
            Price = 50m,
            Unit = "bucket",
            CalculatorEnabled = false
        }
    ];

    private static DemoSettings SeedSettings() => new()
    {
        EnableCalculator = true,
        Stores =
        [
            new StoreOption { Id = "uk", Name = "UK Flooring Store", Enabled = true },
            new StoreOption { Id = "ie", Name = "Ireland Flooring Store", Enabled = true },
            new StoreOption { Id = "eu", Name = "EU Building Store", Enabled = true }
        ]
    };

    private static Product CopyProduct(Product source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        CartName = source.CartName,
        Type = source.Type,
        Price = source.Price,
        Unit = source.Unit,
        CalculatorKey = source.CalculatorKey,
        CoveragePerPack = source.CoveragePerPack,
        MinimumQuantity = source.MinimumQuantity,
        CalculatorEnabled = source.CalculatorEnabled,
        LinkedProductId = source.LinkedProductId,
        LinkedQuantity = source.LinkedQuantity,
        LinkedPerQuantity = source.LinkedPerQuantity
    };

    private static DemoSettings CopySettings(DemoSettings source) => new()
    {
        EnableCalculator = source.EnableCalculator,
        Stores = source.Stores.Select(store => new StoreOption
        {
            Id = store.Id,
            Name = store.Name,
            Enabled = store.Enabled
        }).ToList()
    };

    private static CartLine CopyLine(CartLine source) => new()
    {
        ProductId = source.ProductId,
        Name = source.Name,
        Quantity = source.Quantity,
        UnitPrice = source.UnitPrice,
        LineTotal = source.LineTotal
    };
}
