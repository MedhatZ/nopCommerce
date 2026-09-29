using Nop.Plugin.Widgets.ProductCalculator;
using Nop.Plugin.Widgets.ProductCalculator.Calculators;
using Nop.Plugin.Widgets.ProductCalculator.Models;
using NUnit.Framework;

namespace Nop.Plugin.Widgets.ProductCalculator.Tests;

public class FlooringCalculatorTests
{
    private readonly FlooringCalculator _calculator = new();

    [Test]
    public void FiveByFour_WithCoverageTwoAndAHalf_ReturnsEightPacks()
    {
        var result = _calculator.Calculate(5m, 4m, 2.5m);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Packs, Is.EqualTo(8));
    }

    [Test]
    public void OneByOne_WithCoverageTwoAndAHalf_ReturnsOnePack()
    {
        var result = _calculator.Calculate(1m, 1m, 2.5m);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Packs, Is.EqualTo(1));
    }

    [Test]
    public void InvalidDimensions_ReturnsError()
    {
        var result = _calculator.Calculate(-5m, 0m, 2.5m);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Packs, Is.EqualTo(0));
        Assert.That(result.Error, Is.EqualTo(ProductCalculatorDefaults.InvalidDimensions));
    }

    [Test]
    public void RequestContract_DoesNotAcceptQuantity()
    {
        var quantity = typeof(CalculatorRequest).GetProperty("Quantity");

        Assert.That(quantity, Is.Null);
    }
}
