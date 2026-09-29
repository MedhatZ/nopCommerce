using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.ProductCalculator.Models;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.ProductCalculator.Components;

public class ProductCalculatorViewComponent : NopViewComponent
{
    private readonly IProductService _productService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;

    public ProductCalculatorViewComponent(
        IProductService productService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _productService = productService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (additionalData is not ProductDetailsModel product)
            return Content(string.Empty);

        var store = await _storeContext.GetCurrentStoreAsync();
        var settings = await _settingService.LoadSettingAsync<ProductCalculatorSettings>(store.Id);
        if (!settings.EnableCalculator || settings.Coverage <= 0)
            return Content(string.Empty);

        var linkedName = string.Empty;
        if (settings.LinkedProductId > 0)
            linkedName = (await _productService.GetProductByIdAsync(settings.LinkedProductId))?.Name ?? string.Empty;

        var model = new CalculatorWidgetModel
        {
            ProductId = product.Id,
            Coverage = settings.Coverage.ToString("0.##", CultureInfo.InvariantCulture),
            LinkedProductName = linkedName
        };

        return View("~/Plugins/Widgets.ProductCalculator/Views/PublicInfo.cshtml", model);
    }
}
