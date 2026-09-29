using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CalculatorBridge.Demo.Tests;

public class CalculatorApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CalculatorApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddToCart_ComputesQuantityOnTheServer()
    {
        var client = await SignedInClient();

        var response = await client.PostAsJsonAsync("/api/calculator/add-to-cart", new
        {
            productId = 100,
            length = 5,
            width = 4,
            quantity = 1,
            coverage = 100
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(20, body.GetProperty("area").GetDecimal());
        Assert.Equal(2.5m, body.GetProperty("coveragePerPack").GetDecimal());
        Assert.Equal(8, body.GetProperty("quantity").GetInt32());
        Assert.Equal("Ceiling(20 / 2.5) = 8", body.GetProperty("formula").GetString());
        Assert.True(body.GetProperty("clientQuantityIgnored").GetBoolean());
        Assert.Equal(1, body.GetProperty("discardedClientInput").GetProperty("quantity").GetInt32());
        Assert.Equal(100, body.GetProperty("discardedClientInput").GetProperty("coverage").GetDecimal());

        var lines = body.GetProperty("lines");
        Assert.Equal("Oak Flooring Pack", lines[0].GetProperty("name").GetString());
        Assert.Equal(8, lines[0].GetProperty("quantity").GetInt32());
        Assert.Equal("Adhesive Bucket", lines[1].GetProperty("name").GetString());
        Assert.Equal(1, lines[1].GetProperty("quantity").GetInt32());
        Assert.Equal(410, body.GetProperty("total").GetDecimal());

        var cart = await client.GetFromJsonAsync<JsonElement>("/api/cart");
        Assert.Equal(410, cart.GetProperty("total").GetDecimal());
        Assert.Equal(2, cart.GetProperty("lines").GetArrayLength());
    }

    [Fact]
    public async Task AddToCart_RejectsInvalidDimensionsWithoutChangingTheCart()
    {
        var client = await SignedInClient();

        var response = await client.PostAsJsonAsync("/api/calculator/add-to-cart", new
        {
            productId = 100,
            length = -5,
            width = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Invalid dimensions", body.GetProperty("error").GetString());
        Assert.Equal("Please enter valid values", body.GetProperty("detail").GetString());

        var cart = await client.GetFromJsonAsync<JsonElement>("/api/cart");
        Assert.Equal(0, cart.GetProperty("lines").GetArrayLength());
        Assert.Equal(0, cart.GetProperty("total").GetDecimal());
    }

    private async Task<HttpClient> SignedInClient()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "demo@nopdemo.com",
            password = "demo123"
        });
        login.EnsureSuccessStatusCode();
        var payload = await login.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", payload.GetProperty("token").GetString());
        return client;
    }
}
