namespace CalculatorBridge.Demo.Models;

public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string CartName { get; set; } = "";
    public string Type { get; set; } = "";
    public decimal Price { get; set; }
    public string Unit { get; set; } = "";
    public string? CalculatorKey { get; set; }
    public decimal CoveragePerPack { get; set; }
    public int MinimumQuantity { get; set; } = 1;
    public bool CalculatorEnabled { get; set; }
    public int? LinkedProductId { get; set; }
    public int LinkedQuantity { get; set; } = 1;
    public int LinkedPerQuantity { get; set; } = 10;

    public string StatusLabel =>
        string.Equals(Type, "Accessory", StringComparison.OrdinalIgnoreCase)
            ? "Linked"
            : CalculatorEnabled ? "Enabled" : "Disabled";
}

public sealed class StoreOption
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public bool Enabled { get; set; }
}

public sealed class DemoSettings
{
    public bool EnableCalculator { get; set; } = true;
    public List<StoreOption> Stores { get; set; } = [];
}

public sealed class CartLine
{
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class AddToCartRequest
{
    public int ProductId { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }

    /// <summary>
    /// Accepted so a tampered client can be demonstrated. Never used to decide quantity.
    /// </summary>
    public int? Quantity { get; set; }

    /// <summary>
    /// Accepted so a tampered client can be demonstrated. Coverage always comes from product configuration.
    /// </summary>
    public decimal? Coverage { get; set; }

    public string? StoreId { get; set; }
}

public sealed class CalculationSuccess
{
    public decimal Area { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal CoveragePerPack { get; set; }
    public int Quantity { get; set; }
    public string Formula { get; set; } = "";
    public string Calculator { get; set; } = "";
    public bool ClientQuantityIgnored { get; set; } = true;
    public DiscardedClientInput? DiscardedClientInput { get; set; }
    public List<CartLine> Lines { get; set; } = [];
    public decimal Total { get; set; }
}

public sealed class DiscardedClientInput
{
    public int? Quantity { get; set; }
    public decimal? Coverage { get; set; }
}

public sealed class CalculationOutcome
{
    public bool IsValid { get; init; }
    public int StatusCode { get; init; } = StatusCodes.Status200OK;
    public string? Error { get; init; }
    public string? Detail { get; init; }
    public CalculationSuccess? Success { get; init; }

    public static CalculationOutcome Fail(int statusCode, string error, string detail) =>
        new()
        {
            IsValid = false,
            StatusCode = statusCode,
            Error = error,
            Detail = detail
        };

    public static CalculationOutcome Ok(CalculationSuccess success) =>
        new()
        {
            IsValid = true,
            Success = success
        };
}

public sealed class ProductConfigurationRequest
{
    public string? CalculatorKey { get; set; }
    public decimal CoveragePerPack { get; set; }
    public int MinimumQuantity { get; set; }
    public bool EnableCalculator { get; set; }
    public int? LinkedProductId { get; set; }
    public int LinkedQuantity { get; set; } = 1;
    public int LinkedPerQuantity { get; set; } = 10;
}

public sealed class LoginRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public sealed class SettingsRequest
{
    public bool EnableCalculator { get; set; }
    public List<StoreOption>? Stores { get; set; }
}
