using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.ProductCalculator.Models;

public record ConfigurationModel : BaseNopModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.ProductCalculator.EnableCalculator")]
    public bool EnableCalculator { get; set; }
    public bool EnableCalculator_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.ProductCalculator.CalculatorType")]
    public int CalculatorTypeId { get; set; }
    public bool CalculatorTypeId_OverrideForStore { get; set; }
    public IList<SelectListItem> AvailableCalculatorTypes { get; set; } = new List<SelectListItem>();

    [NopResourceDisplayName("Plugins.Widgets.ProductCalculator.Coverage")]
    public decimal Coverage { get; set; }
    public bool Coverage_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.ProductCalculator.LinkedProduct")]
    public int LinkedProductId { get; set; }
    public bool LinkedProductId_OverrideForStore { get; set; }
    public IList<SelectListItem> AvailableProducts { get; set; } = new List<SelectListItem>();
}
