# Assertion & Testing Framework Adapters

Fake4Dataverse ships with a built-in operation log and fluent assertions API. For teams that already use a specific assertion or mocking framework, companion NuGet packages integrate seamlessly so you can keep your existing test style.

| Package | Purpose |
|---|---|
| *(built-in)* | Operation log queries + chainable `Should()` assertions |
| `Fake4Dataverse.FluentAssertions` | FluentAssertions integration (`because` messages, `AndConstraint`) |
| `Fake4Dataverse.AwesomeAssertions` | AwesomeAssertions integration (same API shape as FluentAssertions) |
| `Fake4Dataverse.Shouldly` | Shouldly-style `ShouldHaveCreated` / `ShouldNotHaveDeleted` extensions |
| `Fake4Dataverse.Moq` | Wraps the fake engine behind a `Mock<IOrganizationService>` for `Verify()` |
| `Fake4Dataverse.FakeItEasy` | Wraps the fake engine behind a FakeItEasy fake for `MustHaveHappened()` |
| `Fake4Dataverse.Spkl` | Auto-registers `[CrmPluginRegistration]`-decorated plugins into the pipeline |

---

## Built-In Operation Log

The `OperationLog` records every service call when `EnableOperationLog = true` (the default).

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

// Query the log directly
IReadOnlyList<OperationRecord> all = service.OperationLog.Records;

// Boolean helpers
bool created   = service.OperationLog.HasCreated("account");
bool createdId = service.OperationLog.HasCreated("account", id);
bool updated   = service.OperationLog.HasUpdated("account", id);
bool deleted   = service.OperationLog.HasDeleted("account", id);
bool executed  = service.OperationLog.HasExecuted<WhoAmIRequest>();
bool byName    = service.OperationLog.HasExecuted("WhoAmI");

// Filtered lists
IReadOnlyList<OperationRecord> creates = service.OperationLog.GetOperations("Create");
IReadOnlyList<OperationRecord> acctUpdates = service.OperationLog.GetOperations("Update", "account");

// Global log also records all operations across sessions
var globalCreates = env.OperationLog.GetOperations("Create");

// Reset between tests
service.OperationLog.Clear();
```

### OperationRecord Properties

Each `OperationRecord` exposes:

| Property | Type | Description |
|---|---|---|
| `OperationType` | `string` | `"Create"`, `"Update"`, `"Delete"`, `"Retrieve"`, `"RetrieveMultiple"`, `"Execute"`, `"Associate"`, `"Disassociate"` |
| `EntityName` | `string?` | Logical name of the entity, or `null` for entity-less operations |
| `EntityId` | `Guid?` | Record ID, or `null` when not applicable |
| `Timestamp` | `DateTime` | UTC time the operation was recorded |
| `Entity` | `Entity?` | Deep-cloned snapshot of the entity at operation time |
| `Request` | `OrganizationRequest?` | The request object (for `Execute` operations) |

---

## Built-In Fluent Assertions

No extra package is needed. Call `Should()` on any `FakeOrganizationService` to start a chainable assertion:

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
service.Update(new Entity("account", id) { ["name"] = "Contoso Ltd" });

service.Should()
    .HaveCreated("account")
    .HaveCreated("account", id)
    .HaveUpdated("account", id)
    .HaveUpdated("account", id, ("name", "Contoso Ltd"))   // assert attribute values
    .NotHaveDeleted("account", id)
    .NotHaveCreated("contact")
    .NotHaveExecuted<WhoAmIRequest>();
```

All methods return `FakeOrganizationServiceAssertions`, so calls chain naturally. A failed assertion throws `FakeServiceAssertionException`.

### Available Built-In Assertion Methods

| Method | Description |
|---|---|
| `HaveCreated(entityName)` | A Create was recorded for the entity type |
| `HaveCreated(entityName, id)` | A Create was recorded for the entity type and ID |
| `HaveUpdated(entityName, id)` | An Update was recorded for the entity and ID |
| `HaveUpdated(entityName, id, params (string, object?)[])` | Update with specific attribute values |
| `HaveDeleted(entityName, id)` | A Delete was recorded |
| `HaveExecuted<TRequest>()` | An Execute with the request type was recorded |
| `HaveExecuted(requestName)` | An Execute with the request name was recorded |
| `NotHaveCreated(entityName)` | No Create was recorded for the entity type |
| `NotHaveDeleted(entityName, id)` | No Delete was recorded |
| `NotHaveExecuted<TRequest>()` | No Execute with the request type was recorded |

---

## Fake4Dataverse.FluentAssertions

```
dotnet add package Fake4Dataverse.FluentAssertions
```

Integrates with the [FluentAssertions](https://fluentassertions.com/) framework. The `Should()` extension returns a `FakeOrganizationServiceFluentAssertions` object that inherits from `ReferenceTypeAssertions`, giving you `because` parameters and `AndConstraint` chaining:

```csharp
using FluentAssertions;
using Fake4Dataverse.FluentAssertions;

service.Should()
    .HaveCreated("account", id, because: "the service layer must persist new accounts")
    .And
    .HaveUpdated("account", id)
    .And
    .HaveExecuted<WhoAmIRequest>();
```

> **Note:** The FluentAssertions `Should()` extension hides the built-in one when both `using` directives are present. Use only one namespace per test file.

---

## Fake4Dataverse.AwesomeAssertions

```
dotnet add package Fake4Dataverse.AwesomeAssertions
```

Same API shape as the FluentAssertions adapter, built for [AwesomeAssertions](https://github.com/AwesomeAssertions/AwesomeAssertions) (the community fork):

```csharp
using AwesomeAssertions;
using Fake4Dataverse.AwesomeAssertions;

service.Should()
    .HaveCreated("account", id)
    .And
    .HaveUpdated("account", id)
    .And
    .HaveDeleted("account", id)
    .And
    .HaveExecuted<WhoAmIRequest>();
```

---

## Fake4Dataverse.Shouldly

```
dotnet add package Fake4Dataverse.Shouldly
```

[Shouldly](https://docs.shouldly.org/)-style extension methods that read like natural language. Methods return the `FakeOrganizationService` itself, so assertions chain directly:

```csharp
using Fake4Dataverse.Shouldly;

service
    .ShouldHaveCreated("account")
    .ShouldHaveCreated("account", id)
    .ShouldHaveUpdated("account", id)
    .ShouldHaveDeleted("account", id)
    .ShouldHaveExecuted<WhoAmIRequest>()
    .ShouldHaveExecuted("WhoAmI")
    .ShouldNotHaveCreated("contact")
    .ShouldNotHaveDeleted("account", id)
    .ShouldNotHaveExecuted<WhoAmIRequest>();
```

---

## Fake4Dataverse.Moq

```
dotnet add package Fake4Dataverse.Moq
```

Wraps the fake engine behind a `Mock<IOrganizationService>`. All calls are delegated to the in-memory fake, but you can use Moq's `Verify()` family for interaction-based assertions:

```csharp
using Fake4Dataverse.Moq;
using Moq;

var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
Mock<IOrganizationService> mock = service.AsMock();

// Pass mock.Object to production code that expects IOrganizationService
mock.Object.Create(new Entity("account") { ["name"] = "Contoso" });

// Assert with Moq
mock.Verify(m => m.Create(It.IsAny<Entity>()), Times.Once);
mock.Verify(m => m.Delete(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
```

For async-first code paths (`IOrganizationServiceAsync2`), use `AsMockAsync()`:

```csharp
using Microsoft.PowerPlatform.Dataverse.Client;

var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
Mock<IOrganizationServiceAsync2> mockAsync = service.AsMockAsync();

var id = await mockAsync.Object.CreateAsync(
    new Entity("account") { ["name"] = "Contoso" },
    CancellationToken.None);

mockAsync.Verify(m => m.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Once);
```

For plugin testing, create a mock `IOrganizationServiceFactory`:

```csharp
Mock<IOrganizationServiceFactory> factory = service.AsMockFactory();
// factory.Object.CreateOrganizationService(userId) returns the fake service
```

---

## Fake4Dataverse.FakeItEasy

```
dotnet add package Fake4Dataverse.FakeItEasy
```

Same concept as the Moq adapter, using [FakeItEasy](https://fakeiteasy.github.io/) instead:

```csharp
using Fake4Dataverse.FakeItEasy;
using FakeItEasy;

var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
IOrganizationService fake = service.AsFake();

fake.Create(new Entity("account") { ["name"] = "Contoso" });

A.CallTo(() => fake.Create(A<Entity>.Ignored)).MustHaveHappenedOnceExactly();
A.CallTo(() => fake.Delete(A<string>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
```

For async-first code paths (`IOrganizationServiceAsync2`), use `AsFakeAsync()`:

```csharp
using Microsoft.PowerPlatform.Dataverse.Client;

var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
IOrganizationServiceAsync2 fakeAsync = service.AsFakeAsync();

var id = await fakeAsync.CreateAsync(
    new Entity("account") { ["name"] = "Contoso" },
    CancellationToken.None);

A.CallTo(() => fakeAsync.CreateAsync(A<Entity>.Ignored, A<CancellationToken>.Ignored))
    .MustHaveHappenedOnceExactly();
```

For plugin testing with `IOrganizationServiceFactory`:

```csharp
IOrganizationServiceFactory factory = service.AsFakeFactory();
// Returns the fake service for any caller ID
```

---

## Fake4Dataverse.Spkl

```
dotnet add package Fake4Dataverse.Spkl
```

Auto-registers plugins decorated with `[CrmPluginRegistration]` (the SPKL convention) into the fake pipeline. Dispose the result to unregister all steps:

```csharp
using Fake4Dataverse.Spkl;

var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();

using var reg = env.RegisterSpklPluginsFromAssembly(typeof(MyPlugin).Assembly);
// All [CrmPluginRegistration]-decorated IPlugin types are now registered

// Trigger them through normal operations
service.Create(new Entity("account") { ["name"] = "Contoso" });

// Inspect skipped registrations (e.g. workflow activities, custom APIs)
foreach (var skip in reg.SkippedRegistrations)
{
    Console.WriteLine($"{skip.PluginType.Name}: {skip.Reason}");
}
```

You can also register specific types instead of scanning a whole assembly:

```csharp
using var reg = env.RegisterSpklPlugins(typeof(MyPlugin), typeof(AnotherPlugin));
```

---

## Choosing an Adapter

| Scenario | Recommended Adapter |
|---|---|
| No framework preference / minimal dependencies | Built-in `Should()` assertions |
| Team already uses FluentAssertions | `Fake4Dataverse.FluentAssertions` |
| Team already uses AwesomeAssertions | `Fake4Dataverse.AwesomeAssertions` |
| Team already uses Shouldly | `Fake4Dataverse.Shouldly` |
| Existing codebase uses Moq for verification | `Fake4Dataverse.Moq` |
| Existing codebase uses FakeItEasy | `Fake4Dataverse.FakeItEasy` |
| Plugin testing with SPKL-style attributes | `Fake4Dataverse.Spkl` |

The assertion adapters (FluentAssertions, AwesomeAssertions, Shouldly) provide richer failure messages from their respective frameworks. The mocking adapters (Moq, FakeItEasy) are best when your production code already receives `Mock<IOrganizationService>` or you want `Verify()` / `MustHaveHappened()` style assertions alongside the in-memory fake engine.

All adapters can be combined with the built-in `OperationLog` for maximum flexibility.
