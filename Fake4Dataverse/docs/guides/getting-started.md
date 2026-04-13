# Getting Started with Fake4Dataverse

Fake4Dataverse provides an in-memory fake `IOrganizationService` and `IOrganizationServiceAsync2` for unit testing Dataverse / Dynamics 365 applications — no live connection required.

## Installation

Install the core package:

```
dotnet add package Fake4Dataverse
```

The core package includes early-bound metadata registration and file-based data provider helpers. Optional companion packages add assertion adapters and plugin-registration support:

| Package | Purpose |
|---|---|
| `Fake4Dataverse.FluentAssertions` | FluentAssertions `Should()` extensions |
| `Fake4Dataverse.AwesomeAssertions` | AwesomeAssertions `Should()` extensions |
| `Fake4Dataverse.Shouldly` | Shouldly `ShouldHave…` extensions |
| `Fake4Dataverse.FakeItEasy` | FakeItEasy `AsFake()` wrapper |
| `Fake4Dataverse.Moq` | Moq wrapper |
| `Fake4Dataverse.Spkl` | Auto-register plugins from SPKL attributes |

## Quick Start — Basic CRUD

```csharp
using Fake4Dataverse;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();

// Create
var id = service.Create(new Entity("account")
{
    ["name"] = "Contoso Ltd",
    ["revenue"] = new Money(1000000m)
});

// Retrieve
var account = service.Retrieve("account", id, new ColumnSet(true));

// Update
service.Update(new Entity("account", id) { ["name"] = "Contoso Corp" });

// Delete
service.Delete("account", id);
```

## Quick Start — Async Service Interface

```csharp
using Fake4Dataverse;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Threading;

var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
var asyncService = (IOrganizationServiceAsync2)service;

var id = await asyncService.CreateAsync(
    new Entity("account") { ["name"] = "Contoso Ltd" },
    CancellationToken.None);

var account = await asyncService.RetrieveAsync(
    "account", id, new ColumnSet("name"),
    CancellationToken.None);
```

## Quick Start — Writing a Unit Test

```csharp
using Fake4Dataverse;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

public class AccountServiceTests
{
    [Fact]
    public void CreateAccount_SetsNameAndRevenue()
    {
        // Arrange
        var env = new FakeDataverseEnvironment();
        var service = env.CreateOrganizationService();
        var myService = new AccountService(service);

        // Act
        var id = myService.CreateAccount("Contoso", 1000000m);

        // Assert
        var account = service.Retrieve("account", id, new ColumnSet(true));
        Assert.Equal("Contoso", account["name"]);
    }
}
```

## Configuration Presets

Three presets cover common scenarios:

```csharp
var env = new FakeDataverseEnvironment();                                         // Default
var strict  = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Strict);   // Metadata + security on
var lenient = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Lenient);   // All auto-behaviors off

var service = env.CreateOrganizationService();
var strictService = strict.CreateOrganizationService();
var lenientService = lenient.CreateOrganizationService();
```

Options can also be loaded from JSON, a file, or environment variables:

```csharp
var options = FakeOrganizationServiceOptions.FromJsonFile("test-config.json");
var options = FakeOrganizationServiceOptions.FromEnvironment(); // prefix: FAKE4DATAVERSE_
```

See [Configuration Reference](../reference/configuration.md) for the full options table and loading details.

## Seeding Test Data

Use `Seed()` to bulk-insert entities **without** triggering the pipeline:

```csharp
env.Seed(
    new Entity("account") { ["name"] = "Contoso" },
    new Entity("account") { ["name"] = "Fabrikam" }
);
```

Inline data strings and file-based seeding are also supported:

```csharp
env.SeedFromJson(jsonString);
env.SeedFromCsv(csvString);
```

File-based and inline-string helpers live in a companion namespace. Add the package reference and
the `using` directive:

```
dotnet add package Fake4Dataverse
```

```csharp
using Fake4Dataverse.DataProviders; // required for SeedFromJsonFile, SeedFromCsvFile, SeedFromJsonString

env.SeedFromJsonFile("testdata/accounts.json");   // loads from a .json file
env.SeedFromCsvFile("testdata/accounts.csv");     // loads from a .csv file
env.SeedFromJsonString("{\"logicalname\":\"account\",\"name\":\"Contoso\"}"); // inline flat JSON
```

The **flat JSON format** used by `SeedFromJsonFile` / `SeedFromCsvFile` / `SeedFromJsonString` is
different from the nested format used by `env.SeedFromJson()`:

| Method | Format | ID |
|---|---|---|
| `env.SeedFromJson()` | `[{"logicalName":"account","id":"...","attributes":{"name":"Contoso"}}]` | Explicit |
| `SeedFromJsonFile()` / `SeedFromJsonString()` | `{"logicalname":"account","name":"Contoso"}` (flat, top-level attributes) | Auto-generated |

CSV files follow the header format `logicalName[,id],attr1,attr2,...` and support RFC-4180
quoted fields — `"Contoso, Ltd."` is parsed as a single value even though it contains a comma.

For complex entities, the fluent `EntityBuilder` keeps setup readable:

```csharp
var account = new EntityBuilder("account")
    .WithName("Contoso")
    .WithState(0)
    .Build();
```

More patterns in the [Cookbook](cookbook.md).

## CRUD Behavior Details

### Primary ID Attribute Set on Create

After `service.Create(entity)` returns, the primary ID attribute on the **passed-in `Entity` object**
is automatically set to the new GUID:

```csharp
var entity = new Entity("account") { ["name"] = "Contoso" };
var id = service.Create(entity);

// entity["accountid"] is now equal to id
Assert.Equal(id, entity.GetAttributeValue<Guid>("accountid"));
```

This mirrors real Dataverse behaviour and is useful when plugin code accesses `entity.Id` after
calling `Create`.

### `overriddencreatedon` — Historical Dates on Import

If a record being created has the `overriddencreatedon` attribute set, that value is used as the
`createdon` timestamp instead of the current clock reading. This allows you to seed historical data
with accurate creation dates:

```csharp
var historicalDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
service.Create(new Entity("account")
{
    ["name"]              = "Contoso",
    ["overriddencreatedon"] = historicalDate
});

var account = service.Retrieve("account", id, new ColumnSet("createdon"));
Assert.Equal(historicalDate, account.GetAttributeValue<DateTime>("createdon"));
```

### Empty String Attribute Stripping

Attribute values set to an empty string (`""`) are removed from the entity before storage — this is
consistent with real Dataverse behaviour where empty strings are `null` at the platform level:

```csharp
service.Create(new Entity("account") { ["description"] = "" });
var saved = service.Retrieve("account", id, new ColumnSet("description"));

Assert.False(saved.Contains("description")); // attribute was stripped
```

This applies on both `Create` and `Update`.

---

## Status Transitions

By default the fake accepts any `statecode`/`statuscode` combination in a `SetStateRequest`. You
can restrict which transitions are valid using `RegisterStatusTransition`:

```csharp
var env = new FakeDataverseEnvironment();

// Only allow: Active(0/1) → Inactive(1/2), and Inactive(1/2) → Active(0/1)
env.RegisterStatusTransition("account", fromStateCode: 0, fromStatusCode: 1, toStateCode: 1, toStatusCode: 2);
env.RegisterStatusTransition("account", fromStateCode: 1, fromStatusCode: 2, toStateCode: 0, toStatusCode: 1);

var service = env.CreateOrganizationService();
var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

// Valid transition — succeeds
service.Execute(new SetStateRequest
{
    EntityMoniker = new EntityReference("account", id),
    State         = new OptionSetValue(1),
    Status        = new OptionSetValue(2)
});

// Invalid transition — throws FaultException
Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
    service.Execute(new SetStateRequest
    {
        EntityMoniker = new EntityReference("account", id),
        State         = new OptionSetValue(0),
        Status        = new OptionSetValue(99) // not registered
    }));
```

Once any transition is registered for an entity type, **all** transitions must be registered;
unregistered combinations throw a fault.

### Custom Default Status Codes

Override the `statecode`/`statuscode` that are set automatically on `Create` when
`AutoSetStateCode = true`:

```csharp
// Make new "incident" records start as "In Progress" (statecode=0, statuscode=2)
// instead of the generic default (statecode=0, statuscode=1)
env.RegisterDefaultStatusCode("incident", stateCode: 0, statusCode: 2);
```

---

## Controlling the Clock

### FakeClock — Advancing Time

```csharp
var clock = new FakeClock(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
var env = new FakeDataverseEnvironment();
env.Clock = clock;

var service = env.CreateOrganizationService();
var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

// Advance by 1 day
clock.Advance(TimeSpan.FromDays(1));
service.Update(new Entity("account", id) { ["name"] = "Contoso Corp" });

var account = service.Retrieve("account", id, new ColumnSet("createdon", "modifiedon"));
Assert.Equal(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), account.GetAttributeValue<DateTime>("createdon"));
Assert.Equal(new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc), account.GetAttributeValue<DateTime>("modifiedon"));
```

### FakeClock.Set — Jump to a Specific Time

`Set()` mutates the existing clock instance. This is useful when a helper already holds a
reference to the clock and you want to jump to a specific point in time rather than advance
incrementally:

```csharp
var clock = new FakeClock();
env.Clock = clock;

clock.Set(new DateTime(2030, 12, 31, 23, 59, 59, DateTimeKind.Utc));
// env.Clock.UtcNow is now 2030-12-31 23:59:59 UTC
```

Using `clock.Set()` is equivalent to assigning a new `FakeClock` value to `env.Clock` _except_
that it preserves the reference — any code that captured `env.Clock` before the `Set()` call
also sees the update.

---

## Resetting the Environment

`env.Reset()` wipes **all data** from the in-memory store (entities and binary attributes). It does
**not** clear metadata registrations, pipeline steps, or security role assignments:

```csharp
env.Reset();
// All records gone; metadata, pipeline, and security still intact.
```

For per-test isolation, prefer `env.Scope()` (auto-rollback) or `env.TakeSnapshot()` /
`env.RestoreSnapshot()` — these are lighter-weight and preserve the starting state. Use `Reset()`
only when you want a completely clean slate between test classes or test suites.

| Method | What is rolled back | Registration state |
|---|---|---|
| `env.Scope()` | All data (entities + binary) since snapshot | Unchanged |
| `env.RestoreSnapshot(snap)` | All data (entities + binary) to save-point | Unchanged |
| `env.Reset()` | All data (entities + binary) — no restore point | Unchanged |

---

## Test Isolation

**Scoped auto-rollback** — all changes inside the `using` block are reverted on dispose:

```csharp
using (env.Scope())
{
    service.Create(new Entity("account") { ["name"] = "Temp" });
    // rolled back automatically
}
```

**Snapshot / Restore** — manual save-point for more control:

```csharp
env.TakeSnapshot();
// ... make changes ...
env.RestoreSnapshot(); // reverts to snapshot
```

## Time Control

Pin the clock for deterministic timestamps:

```csharp
env.Clock = new FakeClock(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));

var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
// createdon == 2024-01-01

env.AdvanceTime(TimeSpan.FromDays(30));
// Now == 2024-01-31
```

## Repository Samples

The `samples/` directory contains runnable projects you can build and test locally:

| Project | Description |
|---|---|
| `Fake4Dataverse.Samples.AccountService` | Service-layer code consuming `IOrganizationService` |
| `Fake4Dataverse.Samples.AccountService.Tests` | Unit tests: CRUD, scope rollback, query joins |
| `Fake4Dataverse.Samples.Plugin` | `IPlugin` that creates a related contact |
| `Fake4Dataverse.Samples.Plugin.Tests` | Pipeline registration and plugin verification |

## Next Steps

Detailed topic guides:

- [Pipeline & Plugin Testing](pipeline-plugin-testing.md)
- [Security & Access Control](security-access-control.md)
- [Metadata & Validation](metadata-validation.md)
- [Querying](querying.md)
- [Calculated & Rollup Fields](calculated-rollup-fields.md)
- [Currency & Exchange Rates](currency-exchange-rates.md)
- [Binary & File Operations](binary-file-operations.md)
- [Solution-Aware Entities](solution-aware-entities.md)
- [Performance & Indexing](performance-indexing.md)
- [Assertion Adapters](assertion-adapters.md)
- [Cookbook](cookbook.md)
- [Migration from FakeXrmEasy](migration-from-fakexrmeasy.md)

Reference:

- [Configuration Reference](../reference/configuration.md)
- [Condition Operators](../reference/condition-operators.md)
- [Request Handlers](../reference/request-handlers.md)
- [API Quick Reference](../reference/api-reference.md)
- [Feature Comparison](../reference/comparison.md)
- [Performance Benchmarks](../reference/performance.md)
