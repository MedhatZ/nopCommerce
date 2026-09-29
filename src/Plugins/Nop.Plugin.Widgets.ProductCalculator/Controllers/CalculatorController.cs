using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.ProductCalculator.Models;
using Nop.Plugin.Widgets.ProductCalculator.Services;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Widgets.ProductCalculator.Controllers;

public class CalculatorController : BasePluginController
{
    private readonly ProductCalculatorCartService _calculatorCartService;

    public CalculatorController(ProductCalculatorCartService calculatorCartService)
    {
        _calculatorCartService = calculatorCartService;
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Calculate([FromBody] CalculatorRequest request)
    {
        var outcome = await _calculatorCartService.QuoteAsync(request?.ProductId ?? 0, request?.Length, request?.Width);
        return ToResult(outcome);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> AddToCart([FromBody] CalculatorRequest request)
    {
        var outcome = await _calculatorCartService.AddToCartAsync(request?.ProductId ?? 0, request?.Length, request?.Width);
        return ToResult(outcome);
    }

    private IActionResult ToResult(CalculatorOutcome outcome)
    {
        if (!outcome.Success)
            return StatusCode(outcome.StatusCode, new { error = outcome.Error });

        return Json(new
        {
            packs = outcome.Packs,
            linkedProductName = outcome.LinkedProductName,
            linkedQuantity = outcome.LinkedQuantity
        });
    }
}
