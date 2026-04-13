# Fake4Dataverse

An in-memory fake `IOrganizationService` and `IOrganizationServiceAsync2` for unit testing Dataverse / Dynamics 365 applications — no live connection required.

[![Build & Test](https://github.com/nicknow/Fake4Dataverse/actions/workflows/build.yml/badge.svg)](https://github.com/nicknow/Fake4Dataverse/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Fake4Dataverse.svg)](https://www.nuget.org/packages/Fake4Dataverse)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Features

| Category | Capabilities |
|----------|-------------|
| **CRUD** | Create, Retrieve, Update, Delete with auto-set fields (timestamps, owner, state, version number) |
| **Service interfaces** | `IOrganizationService` and `IOrganizationServiceAsync2` (async + cancellation token support) |
| **QueryExpression** | Filtering (40+ condition operators), ordering, column projection, TopCount, paging with cookies |
| **FetchXml** | FetchXml parsing and in-memory evaluation for the supported query surface, including aggregation (Count, Sum, Avg, Min, Max, GroupBy) |
| **LinkEntity** | Inner/outer joins, nested joins, semi/anti joins (`exists`, `in`, `any`, `not-any`, `not-all`), link criteria, aliased attributes |
| **Pipeline** | Pre-validation, pre-operation, and post-operation hooks (plugin-like) |
| **Metadata** | Entity/attribute metadata store, validation on Create/Update, auto-discovery |
| **Security** | Security roles, privilege enforcement, record sharing (Grant/Modify/Revoke access) |
| **Execute handlers** | WhoAmI, SetState, Assign, Upsert, FetchXml/QueryExpression conversion requests, ExecuteMultiple, ExecuteTransaction, and more |
| **Concurrency** | Optimistic concurrency (`ConcurrencyBehavior.IfRowVersionMatches`), atomic transactions with copy-on-write rollback |
| **Calculated fields** | Calculated and rollup field definitions evaluated on Retrieve |
| **Currency** | Exchange rates and auto-computed base currency amounts |
| **Activity parties** | ActivityParty entity support for from/to/cc/bcc fields |
| **Binary attributes** | Image and file column storage |
| **Seeding** | Bulk insert, JSON seeding, `EntityBuilder` fluent API |
| **Snapshots** | `TakeSnapshot()` / `RestoreSnapshot()` / `Scope()` for test isolation |
| **Time control** | `FakeClock` for deterministic date/time testing |
| **Operation log** | Records all service calls for post-hoc assertions |
| **Configuration** | `FakeOrganizationServiceOptions` with Strict/Lenient presets |
| **Multi-user** | Multiple `FakeOrganizationService` sessions against the same `FakeDataverseEnvironment` |
| **Multi-target** | .NET Framework 4.6.2 and .NET 10 |

Built-in query conversion handlers are registered automatically:
`FetchXmlToQueryExpressionRequest` converts supported non-aggregate FetchXml, and
`QueryExpressionToFetchXmlRequest` serializes the supported `QueryExpression`
surface without silently degrading unsupported operators or join types.

## Installation

```
dotnet add package Fake4Dataverse
```

## Quick Start

### Basic CRUD

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();

var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

var retrieved = service.Retrieve("account", id, new ColumnSet("name"));
Assert.Equal("Contoso", retrieved["name"]);

service.Update(new Entity("account") { Id = id, ["name"] = "Contoso Ltd." });
service.Delete("account", id);
```

### Async Interface (`IOrganizationServiceAsync2`)

```csharp
using Microsoft.PowerPlatform.Dataverse.Client;
using System.Threading;

var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
var asyncService = (IOrganizationServiceAsync2)service;

var id = await asyncService.CreateAsync(
    new Entity("account") { ["name"] = "Contoso" },
    CancellationToken.None);

var account = await asyncService.RetrieveAsync(
    "account", id, new ColumnSet("name"),
    CancellationToken.None);
```

### Test a Real Plugin

```csharp
[Fact]
public void Create_ExecutesRegisteredPlugin()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, "account", new PrefixNamePlugin());

    var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
    var account = service.Retrieve("account", id, new ColumnSet("name"));

    Assert.Equal("PLUGIN: Contoso", account.GetAttributeValue<string>("name"));
}

private sealed class PrefixNamePlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext))!;
        var target = (Entity)context.InputParameters["Target"];
        target["name"] = $"PLUGIN: {target.GetAttributeValue<string>("name")}";
    }
}
```

## Configuration

`FakeOrganizationServiceOptions` controls auto-set behaviors, validation, security, and pipeline.
Use built-in presets for common setups:

```csharp
var strict  = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Strict);   // metadata validation + security on
var lenient = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Lenient);   // everything off — full manual control
var strictService  = strict.CreateOrganizationService();
var lenientService = lenient.CreateOrganizationService();
```

See the [Configuration Reference](docs/reference/configuration.md) for all options.

## Companion Packages

| Package | Description |
|---------|-------------|
| **Fake4Dataverse.FakeItEasy** | Wraps the fake service as a FakeItEasy `Fake<IOrganizationService>` |
| **Fake4Dataverse.Moq** | Wraps the fake service as a Moq `Mock<IOrganizationService>` |
| **Fake4Dataverse.AwesomeAssertions** | `Should().HaveCreated(…)` style assertions via AwesomeAssertions |
| **Fake4Dataverse.FluentAssertions** | `Should().HaveCreated(…)` style assertions via FluentAssertions |
| **Fake4Dataverse.Shouldly** | `ShouldHaveCreated(…)` style assertions via Shouldly |
| **Fake4Dataverse.Spkl** | Auto-register plugins from SPKL `[CrmPluginRegistration]` attributes |

Install any adapter alongside the core package:

```
dotnet add package Fake4Dataverse.AwesomeAssertions
```

## Fake4Dataverse CLI Tool

The `Fake4Dataverse.Tool` dotnet global tool exports real table metadata directly from a Dataverse environment to XML files. Those files can then be loaded in your tests to provide accurate, schema-validated metadata without manual coding.

### Install

```
dotnet tool install --global Fake4Dataverse.Tool
```

### Export metadata by table name

```
fake4dataverse export-metadata \
  --url https://org.crm.dynamics.com/ \
  --tables account contact opportunity \
  --output ./metadata
```

### Export metadata from a solution

```
fake4dataverse export-metadata \
  --url https://org.crm.dynamics.com/ \
  --solutions MySolution AnotherSolution \
  --output ./metadata
```

You can combine `--tables` and `--solutions` freely.

Authentication uses interactive login (browser sign-in / device code). A token cache is stored in `%LOCALAPPDATA%/Fake4Dataverse/` so subsequent runs do not prompt again.

### Load exported files in your tests

Each exported file is a DataContract-serialized `EntityMetadata` XML named `<logicalname>.xml`.
Load one or all files in your test setup:

```csharp
var env = new FakeDataverseEnvironment();

// Load a single table
env.LoadMetadataFromXmlFile("metadata/account.xml");

// Load all files in a folder
foreach (var file in Directory.GetFiles("metadata", "*.xml"))
    env.LoadMetadataFromXmlFile(file);
```

Any table whose metadata includes a `componentstate` attribute is automatically treated as solution-aware (records are staged as unpublished until published). No extra registration call is needed.

See the [Metadata & Validation guide](docs/guides/metadata-validation.md) for full details.

## Documentation

### Guides

- [Getting Started](docs/guides/getting-started.md)
- [Pipeline & Plugin Testing](docs/guides/pipeline-plugin-testing.md)
- [Security & Access Control](docs/guides/security-access-control.md)
- [Metadata & Validation](docs/guides/metadata-validation.md)
- [Querying (QueryExpression & FetchXml)](docs/guides/querying.md)
- [Calculated & Rollup Fields](docs/guides/calculated-rollup-fields.md)
- [Currency & Exchange Rates](docs/guides/currency-exchange-rates.md)
- [Binary & File Operations](docs/guides/binary-file-operations.md)
- [Performance & Indexing](docs/guides/performance-indexing.md)
- [Concurrency & Transactions](docs/guides/concurrency-transactions.md)
- [Assertion Adapters](docs/guides/assertion-adapters.md)
- [Cookbook — Common Patterns](docs/guides/cookbook.md)
- [Migration from FakeXrmEasy](docs/guides/migration-from-fakexrmeasy.md)

### Reference

- [Configuration Reference](docs/reference/configuration.md)
- [Condition Operators](docs/reference/condition-operators.md)
- [Request Handlers](docs/reference/request-handlers.md)
- [API Quick Reference](docs/reference/api-reference.md)
- [Feature Comparison](docs/reference/comparison.md)
- [Performance Benchmarks](docs/reference/performance.md)

## Samples

The `samples/` directory contains runnable examples with matching test projects:

- **AccountService** — production-style service using `IOrganizationService`, with unit tests.
- **Plugin** — real `IPlugin` implementation with pipeline-based tests.

## License

[MIT](LICENSE)
