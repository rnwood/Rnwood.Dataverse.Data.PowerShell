# Currency & Exchange Rates

Fake4Dataverse includes a `CurrencyManager` that auto-computes `_base` fields for Money attributes when an entity has a `transactioncurrencyid` lookup. This mirrors the Dataverse behavior where base-currency amounts are maintained alongside transaction-currency amounts.

Access the manager via `env.Currency`.

---

## Configuring Exchange Rates

Set the organization's base currency and register exchange rates for other currencies:

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();

var usdId = Guid.NewGuid();
var eurId = Guid.NewGuid();

// Set the organization base currency (USD)
env.Currency.BaseCurrencyId = usdId;

// EUR costs 0.85 USD (i.e., 1 EUR = 1/0.85 USD)
env.Currency.SetExchangeRate(eurId, 0.85m);

// Retrieve a rate (returns 1.0 for unknown currencies)
decimal rate = env.Currency.GetExchangeRate(eurId); // 0.85m
```

- **`BaseCurrencyId`** — the GUID of the organization's base currency record.
- **`SetExchangeRate(currencyId, rate)`** — rate is relative to the base currency. Must be positive.
- **`GetExchangeRate(currencyId)`** — returns the configured rate, or `1.0m` for unknown currencies.

---

## Automatic Base Currency Computation

On `Create` and `Update`, for each `Money` attribute on the entity, the `CurrencyManager` automatically sets:

```
{attributeName}_base = value / exchangeRate
```

It also sets the `exchangerate` attribute on the entity if not already present.

This only applies when the entity has a `transactioncurrencyid` lookup attribute.

---

## Complete Example

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();

var usdId = Guid.NewGuid();
var eurId = Guid.NewGuid();

// Configure: USD is base currency, EUR rate is 0.85
env.Currency.BaseCurrencyId = usdId;
env.Currency.SetExchangeRate(eurId, 0.85m);

// Create an opportunity in EUR
var id = service.Create(new Entity("opportunity")
{
    ["transactioncurrencyid"] = new EntityReference("transactioncurrency", eurId),
    ["estimatedvalue"] = new Money(1000m)  // 1000 EUR
});

// Retrieve and verify base currency computation
var record = service.Retrieve("opportunity", id, new ColumnSet(true));

var baseValue = record.GetAttributeValue<Money>("estimatedvalue_base");
// 1000 / 0.85 = 1176.47 (rounded)
Assert.Equal(Math.Round(1000m / 0.85m, 2), Math.Round(baseValue.Value, 2));

// Exchange rate is also stored on the entity
Assert.Equal(0.85m, record.GetAttributeValue<decimal>("exchangerate"));
```

---

## Tips

- **Configure early** — set `BaseCurrencyId` and exchange rates before creating entities with Money fields.
- **Multiple currencies** — register as many exchange rates as needed; each is keyed by the currency record GUID.
- **Unknown currencies default to 1.0** — if no rate is configured, the base value equals the original value.
- **Update recalculates** — changing a Money attribute or `transactioncurrencyid` on Update triggers recomputation of `_base` fields.
