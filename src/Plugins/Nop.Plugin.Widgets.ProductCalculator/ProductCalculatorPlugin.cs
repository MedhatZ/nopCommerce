using Nop.Core.Domain.Cms;
using Nop.Plugin.Widgets.ProductCalculator.Components;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Widgets.ProductCalculator;

public class ProductCalculatorPlugin : BasePlugin, IWidgetPlugin
{
    private readonly ILocalizationService _localizationService;
    private readonly INopUrlHelper _nopUrlHelper;
    private readonly ISettingService _settingService;
    private readonly WidgetSettings _widgetSettings;

    public ProductCalculatorPlugin(
        ILocalizationService localizationService,
        INopUrlHelper nopUrlHelper,
        ISettingService settingService,
        WidgetSettings widgetSettings)
    {
        _localizationService = localizationService;
        _nopUrlHelper = nopUrlHelper;
        _settingService = settingService;
        _widgetSettings = widgetSettings;
    }

    public bool HideInWidgetList => false;

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.ProductDetailsOverviewBottom
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        ArgumentNullException.ThrowIfNull(widgetZone);

        if (widgetZone.Equals(PublicWidgetZones.ProductDetailsOverviewBottom))
            return typeof(ProductCalculatorViewComponent);

        return null;
    }

    public override string GetConfigurationPageUrl()
    {
        return _nopUrlHelper.RouteUrl(ProductCalculatorDefaults.ConfigurationRouteName);
    }

    public override async Task InstallAsync()
    {
        await _settingService.SaveSettingAsync(new ProductCalculatorSettings());

        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(ProductCalculatorDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(ProductCalculatorDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Widgets.ProductCalculator.EnableCalculator"] = "Enable Calculator",
            ["Plugins.Widgets.ProductCalculator.CalculatorType"] = "Calculator Type",
            ["Plugins.Widgets.ProductCalculator.Coverage"] = "Coverage",
            ["Plugins.Widgets.ProductCalculator.LinkedProduct"] = "Linked Product",
            ["Plugins.Widgets.ProductCalculator.LinkedProduct.Hint"] = "When set, the server adds 1 of this product per 10 calculated packs."
        });

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        if (_widgetSettings.ActiveWidgetSystemNames.Contains(ProductCalculatorDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(ProductCalculatorDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _settingService.DeleteSettingAsync<ProductCalculatorSettings>();
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.ProductCalculator");

        await base.UninstallAsync();
    }
}
