using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Widgets.ProductCalculator.Calculators;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Orders;

namespace Nop.Plugin.Widgets.ProductCalculator.Services;

public class ProductCalculatorCartService
{
    private readonly CalculatorProvider _calculatorProvider;
    private readonly IProductService _productService;
    private readonly ISettingService _settingService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;

    public ProductCalculatorCartService(
        CalculatorProvider calculatorProvider,
        IProductService productService,
        ISettingService settingService,
        IShoppingCartService shoppingCartService,
        IStoreContext storeContext,
        IWorkContext workContext)
    {
        _calculatorProvider = calculatorProvider;
        _productService = productService;
        _settingService = settingService;
        _shoppingCartService = shoppingCartService;
        _storeContext = storeContext;
        _workContext = workContext;
    }

    public Task<CalculatorOutcome> QuoteAsync(int productId, decimal? length, decimal? width)
    {
        return CalculateAsync(productId, length, width, addToCart: false);
    }

    public Task<CalculatorOutcome> AddToCartAsync(int productId, decimal? length, decimal? width)
    {
        return CalculateAsync(productId, length, width, addToCart: true);
    }

    private async Task<CalculatorOutcome> CalculateAsync(int productId, decimal? length, decimal? width, bool addToCart)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var settings = await _settingService.LoadSettingAsync<ProductCalculatorSettings>(store.Id);
        if (!settings.EnableCalculator)
            return CalculatorOutcome.Fail(StatusCodes.Status409Conflict, "Calculator is turned off");

        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return CalculatorOutcome.Fail(StatusCodes.Status404NotFound, "Product not found");

        var calculator = _calculatorProvider.Get(settings.CalculatorType);
        var calculation = calculator.Calculate(length ?? 0, width ?? 0, settings.Coverage);
        if (!calculation.IsValid)
            return CalculatorOutcome.Fail(StatusCodes.Status400BadRequest, calculation.Error);

        if (!addToCart)
            return CalculatorOutcome.Ok(calculation.Packs, null, 0);

        var customer = await _workContext.GetCurrentCustomerAsync();
        var warnings = await _shoppingCartService.AddToCartAsync(customer, product,
            ShoppingCartType.ShoppingCart, store.Id, quantity: calculation.Packs);
        if (warnings.Count > 0)
            return CalculatorOutcome.Fail(StatusCodes.Status400BadRequest, string.Join(" ", warnings));

        var linkedName = string.Empty;
        var linkedQuantity = 0;
        if (settings.LinkedProductId > 0 && settings.LinkedProductId != product.Id)
        {
            var linked = await _productService.GetProductByIdAsync(settings.LinkedProductId);
            if (linked != null)
            {
                linkedQuantity = (int)Math.Ceiling(calculation.Packs / (decimal)ProductCalculatorDefaults.PacksPerLinkedItem);
                var linkedWarnings = await _shoppingCartService.AddToCartAsync(customer, linked,
                    ShoppingCartType.ShoppingCart, store.Id, quantity: linkedQuantity);
                if (linkedWarnings.Count > 0)
                    return CalculatorOutcome.Fail(StatusCodes.Status400BadRequest, string.Join(" ", linkedWarnings));

                linkedName = linked.Name;
            }
        }

        return CalculatorOutcome.Ok(calculation.Packs, linkedName, linkedQuantity);
    }
}

public class CalculatorOutcome
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public string Error { get; init; }
    public int Packs { get; init; }
    public string LinkedProductName { get; init; }
    public int LinkedQuantity { get; init; }

    public static CalculatorOutcome Fail(int statusCode, string error) => new()
    {
        Success = false,
        StatusCode = statusCode,
        Error = error
    };

    public static CalculatorOutcome Ok(int packs, string linkedProductName, int linkedQuantity) => new()
    {
        Success = true,
        StatusCode = StatusCodes.Status200OK,
        Packs = packs,
        LinkedProductName = linkedProductName,
        LinkedQuantity = linkedQuantity
    };
}
