using Nop.Services.Plugins;
using Nop.Web.Framework.Events;

namespace Nop.Plugin.Widgets.ProductCalculator.Infrastructure;

public class EventConsumer : BaseAdminMenuCreatedEventConsumer
{
    public EventConsumer(IPluginManager<IPlugin> pluginManager) : base(pluginManager)
    {
    }

    protected override string PluginSystemName => ProductCalculatorDefaults.SystemName;

    protected override string BeforeMenuSystemName => "Settings";
}
