using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.ProductCalculator.Calculators;
using Nop.Plugin.Widgets.ProductCalculator.Services;

namespace Nop.Plugin.Widgets.ProductCalculator.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ICalculator, FlooringCalculator>();
        services.AddSingleton<ICalculator, WallPanelCalculator>();
        services.AddSingleton<CalculatorProvider>();
        services.AddScoped<ProductCalculatorCartService>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 3000;
}
