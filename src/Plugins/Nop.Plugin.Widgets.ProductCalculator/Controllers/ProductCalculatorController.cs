using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Widgets.ProductCalculator.Models;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.ProductCalculator.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class ProductCalculatorController : BasePluginController
{
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IProductService _productService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;

    public ProductCalculatorController(
        ILocalizationService localizationService,
        INotificationService notificationService,
        IProductService productService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _productService = productService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> Configure()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<ProductCalculatorSettings>(storeScope);
        var model = await PrepareModelAsync(settings, storeScope);

        if (storeScope > 0)
        {
            model.EnableCalculator_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableCalculator, storeScope);
            model.CalculatorTypeId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.CalculatorType, storeScope);
            model.Coverage_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Coverage, storeScope);
            model.LinkedProductId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.LinkedProductId, storeScope);
        }

        return View("~/Plugins/Widgets.ProductCalculator/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<ProductCalculatorSettings>(storeScope);

        settings.EnableCalculator = model.EnableCalculator;
        settings.CalculatorType = Enum.IsDefined(typeof(CalculatorType), model.CalculatorTypeId)
            ? (CalculatorType)model.CalculatorTypeId
            : CalculatorType.Flooring;
        settings.Coverage = model.Coverage;
        settings.LinkedProductId = model.LinkedProductId;

        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableCalculator, model.EnableCalculator_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CalculatorType, model.CalculatorTypeId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Coverage, model.Coverage_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.LinkedProductId, model.LinkedProductId_OverrideForStore, storeScope, false);
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    private async Task<ConfigurationModel> PrepareModelAsync(ProductCalculatorSettings settings, int storeScope)
    {
        var products = await _productService.SearchProductsAsync(pageSize: 300, showHidden: true);
        var availableProducts = new List<SelectListItem>
        {
            new("None", "0")
        };
        availableProducts.AddRange(products.Select(product => new SelectListItem(product.Name, product.Id.ToString())));

        if (settings.LinkedProductId > 0 && availableProducts.All(item => item.Value != settings.LinkedProductId.ToString()))
        {
            var selected = await _productService.GetProductByIdAsync(settings.LinkedProductId);
            if (selected != null)
                availableProducts.Add(new SelectListItem(selected.Name, selected.Id.ToString()));
        }

        return new ConfigurationModel
        {
            ActiveStoreScopeConfiguration = storeScope,
            EnableCalculator = settings.EnableCalculator,
            CalculatorTypeId = (int)settings.CalculatorType,
            Coverage = settings.Coverage,
            LinkedProductId = settings.LinkedProductId,
            AvailableCalculatorTypes = new List<SelectListItem>
            {
                new("Flooring", ((int)CalculatorType.Flooring).ToString()),
                new("Wall Panel", ((int)CalculatorType.WallPanel).ToString())
            },
            AvailableProducts = availableProducts
        };
    }
}
