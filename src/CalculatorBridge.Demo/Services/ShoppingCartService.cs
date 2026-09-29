using CalculatorBridge.Demo.Data;
using CalculatorBridge.Demo.Models;

namespace CalculatorBridge.Demo.Services;

public sealed class ShoppingCartService
{
    private readonly DemoStore _store;

    public ShoppingCartService(DemoStore store)
    {
        _store = store;
    }

    public CartSnapshot Get(string token)
    {
        var lines = _store.GetCart(token).ToList();
        return new CartSnapshot(lines, lines.Sum(line => line.LineTotal));
    }

    public CartSnapshot SetCalculatedItems(string token, IReadOnlyList<CartLine> lines)
    {
        _store.SetCart(token, lines);
        return Get(token);
    }
}

public sealed record CartSnapshot(IReadOnlyList<CartLine> Lines, decimal Total);
