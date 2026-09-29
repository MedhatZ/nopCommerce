using CalculatorBridge.Demo.Data;
using CalculatorBridge.Demo.Models;
using CalculatorBridge.Demo.Services;

namespace CalculatorBridge.Demo.Endpoints;

public static class DemoEndpoints
{
    public static WebApplication MapDemoEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        api.MapPost("/auth/login", (LoginRequest request, DemoAuthService auth) =>
        {
            var token = auth.Login(request.Username, request.Password);
            if (token is null)
            {
                return Results.Json(
                    new { error = "Invalid login", detail = "Check the demo username and password." },
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            return Results.Ok(new { token });
        });

        api.MapPost("/auth/logout", (HttpContext context, DemoAuthService auth) =>
        {
            auth.Revoke(Token(context));
            return Results.NoContent();
        });

        api.MapGet("/products", (ProductService products) => products.GetProducts());

        api.MapGet("/products/{id:int}", (int id, ProductService products) =>
        {
            var product = products.GetProduct(id);
            return product is null ? Results.NotFound(new { error = "Product not found" }) : Results.Ok(product);
        });

        api.MapPut("/products/{id:int}/configuration", (int id, ProductConfigurationRequest request, ProductService products) =>
        {
            var (product, error) = products.SaveConfiguration(id, request);
            if (error is not null && product is null && error == "Product not found.")
                return Results.NotFound(new { error });

            if (error is not null)
                return Results.BadRequest(new { error });

            return Results.Ok(new { message = "Configuration saved.", product });
        });

        api.MapGet("/calculators", (ProductService products) => products.GetCalculators());

        api.MapPost("/calculator/quote", (AddToCartRequest request, CalculatorService calculator) =>
            ToResult(calculator.Calculate(request)));

        api.MapPost("/calculator/add-to-cart", (
            AddToCartRequest request,
            HttpContext context,
            CalculatorService calculator,
            ShoppingCartService carts) =>
        {
            var outcome = calculator.Calculate(request);
            if (!outcome.IsValid || outcome.Success is null)
                return ToResult(outcome);

            carts.SetCalculatedItems(Token(context), outcome.Success.Lines);
            return Results.Ok(outcome.Success);
        });

        api.MapGet("/cart", (HttpContext context, ShoppingCartService carts) =>
            carts.Get(Token(context)));

        api.MapGet("/settings", (DemoStore store) => store.GetSettings());

        api.MapPut("/settings", (SettingsRequest request, DemoStore store) =>
        {
            var current = store.GetSettings();
            var requested = request.Stores ?? [];
            if (requested.Count != current.Stores.Count ||
                current.Stores.Any(storeOption => requested.All(item => !string.Equals(item.Id, storeOption.Id, StringComparison.OrdinalIgnoreCase))))
            {
                return Results.BadRequest(new { error = "Settings must include every demo store." });
            }

            current.EnableCalculator = request.EnableCalculator;
            foreach (var storeOption in current.Stores)
            {
                var match = requested.First(item => string.Equals(item.Id, storeOption.Id, StringComparison.OrdinalIgnoreCase));
                storeOption.Enabled = match.Enabled;
            }

            store.SaveSettings(current);
            return Results.Ok(new
            {
                message = "Settings saved.",
                enableCalculator = current.EnableCalculator,
                status = current.EnableCalculator ? "YES" : "NO",
                stores = current.Stores
            });
        });

        api.MapPost("/demo/reset", (DemoStore store) =>
        {
            store.Reset();
            return Results.Ok(new { message = "Demo data restored." });
        });

        return app;
    }

    public static void UseDemoAuth(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path;
            if (!path.StartsWithSegments("/api") || path.StartsWithSegments("/api/auth/login"))
            {
                await next();
                return;
            }

            var auth = context.RequestServices.GetRequiredService<DemoAuthService>();
            var header = context.Request.Headers.Authorization.ToString();
            const string prefix = "Bearer ";
            if (header.StartsWith(prefix, StringComparison.Ordinal))
            {
                var token = header[prefix.Length..].Trim();
                if (auth.IsValid(token))
                {
                    context.Items["token"] = token;
                    await next();
                    return;
                }
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                detail = "Sign in with the demo account."
            });
        });
    }

    private static IResult ToResult(CalculationOutcome outcome)
    {
        if (!outcome.IsValid || outcome.Success is null)
        {
            return Results.Json(
                new { error = outcome.Error, detail = outcome.Detail },
                statusCode: outcome.StatusCode);
        }

        return Results.Ok(outcome.Success);
    }

    private static string Token(HttpContext context) => (string)context.Items["token"]!;
}
