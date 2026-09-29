using CalculatorBridge.Demo.Calculators;
using CalculatorBridge.Demo.Data;
using CalculatorBridge.Demo.Endpoints;
using CalculatorBridge.Demo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DemoStore>();
builder.Services.AddSingleton<ICalculatorStrategy, FlooringCalculatorStrategy>();
builder.Services.AddSingleton<ICalculatorStrategy, WallPanelCalculatorStrategy>();
builder.Services.AddSingleton<DemoAuthService>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddSingleton<CalculatorService>();
builder.Services.AddSingleton<ShoppingCartService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseDemoAuth();
app.MapDemoEndpoints();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program
{
}
