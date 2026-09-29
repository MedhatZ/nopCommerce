# nopCommerce Calculator Bridge — Demo

Runnable proof of a nopCommerce product-page calculator: admin setup, the customer widget, and the cart. The server computes the quantity from stored coverage and the posted room size.

## Demo purpose

Show how a flooring calculator can live beside nopCommerce without editing core code:

- An admin configures coverage, a minimum quantity, and a linked accessory.
- The product page collects room length and width.
- `POST /api/calculator/add-to-cart` computes the area and the pack quantity.
- A posted quantity or coverage is ignored.
- The linked adhesive is added from the saved rule: 1 bucket per 10 packs.
- The same calculator can be enabled per store.

Demo login: `demo@nopdemo.com` / `demo123`.

The sample walkthrough uses Oak Flooring Pack (`productId` 100), a 5 m × 4 m room, and 2.5 m² per pack. The server returns 8 packs and 1 adhesive bucket. The cart total is $410.

## Installation

Requires the .NET 9 SDK.

```bash
dotnet test
dotnet run --project src/CalculatorBridge.Demo
```

Open http://localhost:5080 and sign in with the demo account.

`dotnet test` checks the sample calculation and the invalid-dimension response against the real endpoint.

## Architecture overview

The demo follows the boundary a nopCommerce plugin would use. In the real plugin these services are registered from `INopStartup`, and the widget is rendered by `IWidgetPlugin` on the product page. nopCommerce controllers and views stay untouched.

```
Product Page
      |
Widget Component
      |
Calculator Service
      |
Product Service
      |
Shopping Cart Service
      |
Database
```

- No core modification
- Uses service layer
- Supports multiple calculators
- Secure server-side calculation

| Demo piece | nopCommerce equivalent |
| --- | --- |
| Room calculator widget | `IWidgetPlugin` view component on the product page |
| `CalculatorService` | Plugin service called by the widget and the add-to-cart endpoint |
| `ProductService` | Plugin service over catalog data and calculator configuration |
| `ShoppingCartService` | Plugin service that calls `IShoppingCartService` with the server quantity |
| `DemoStore` | `IRepository<T>` plus `ISettingService` store scope |
| Store checkboxes | Per-store settings via the store scope, not a copied database |

`FlooringCalculatorStrategy` and `WallPanelCalculatorStrategy` both implement `ICalculatorStrategy`. Product configuration chooses which one runs. Linked accessories stay on the product configuration, so a new calculator does not reimplement cart rules.

In this demo, `DemoStore` keeps data in memory. Restarting the app, or choosing Reset demo data, restores the sample catalog.

## API example

Login:

```bash
curl -s http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"demo@nopdemo.com\",\"password\":\"demo123\"}"
```

Calculate and add to cart. Coverage and quantity are not inputs. The server reads coverage from the saved product (2.5 m²) and rounds up.

```bash
curl -s http://localhost:5080/api/calculator/add-to-cart \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"productId\":100,\"length\":5,\"width\":4}"
```

```json
{
  "area": 20,
  "coveragePerPack": 2.5,
  "quantity": 8,
  "formula": "Ceiling(20 / 2.5) = 8",
  "clientQuantityIgnored": true,
  "lines": [
    { "productId": 100, "name": "Oak Flooring Pack", "quantity": 8, "unitPrice": 45, "lineTotal": 360 },
    { "productId": 102, "name": "Adhesive Bucket", "quantity": 1, "unitPrice": 50, "lineTotal": 50 }
  ],
  "total": 410
}
```

`Area = length × width`, then `Quantity = Ceiling(area / coverage)`. If the body also contains `quantity` or `coverage`, those values are returned under `discardedClientInput` and are not used.

Invalid sizes:

```bash
curl -s http://localhost:5080/api/calculator/add-to-cart \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"productId\":100,\"length\":-5,\"width\":0}"
```

```json
{
  "error": "Invalid dimensions",
  "detail": "Please enter valid values"
}
```

`POST /api/calculator/quote` uses the same service and does not change the cart. The product page calls it for Calculate Quantity.

## How to add a new calculator

1. Add a class that implements `ICalculatorStrategy` (`Key`, `DisplayName`, `CalculateQuantity`, `DescribeFormula`).
2. Register it next to the existing strategies in `Program.cs`. In the nopCommerce plugin, register it in `INopStartup.ConfigureServices`.
3. It shows up in the product configuration Calculator Type list.
4. Assign it to a product. The widget and `POST /api/calculator/add-to-cart` resolve it by key. Quantity is still computed only on the server, from stored coverage and the posted length and width.
