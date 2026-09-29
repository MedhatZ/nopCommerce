using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Nop.Plugin.Widgets.ProductCalculator.Infrastructure;

public class RouteProvider : BaseRouteProvider, IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(
            name: ProductCalculatorDefaults.ConfigurationRouteName,
            pattern: "Admin/ProductCalculator/Configure",
            defaults: new { controller = "ProductCalculator", action = "Configure", area = AreaNames.ADMIN });

        endpointRouteBuilder.MapControllerRoute(
            name: ProductCalculatorDefaults.CalculateRouteName,
            pattern: "ProductCalculator/Calculate",
            defaults: new { controller = "Calculator", action = "Calculate" });

        endpointRouteBuilder.MapControllerRoute(
            name: ProductCalculatorDefaults.AddToCartRouteName,
            pattern: "ProductCalculator/AddToCart",
            defaults: new { controller = "Calculator", action = "AddToCart" });
    }

    public int Priority => 0;
}
