# Pipeline & Plugin Testing

## Overview

Fake4Dataverse includes a full plugin-like execution pipeline that mirrors the Dataverse event framework. Every CRUD operation flows through three stages before and after the core operation:

| Stage | Enum Value | Int Value | Purpose |
|---|---|---|---|
| **PreValidation** | `PipelineStage.PreValidation` | 10 | Validate inputs; throw to abort before any work |
| **PreOperation** | `PipelineStage.PreOperation` | 20 | Modify fields before the record is saved |
| **PostOperation** | `PipelineStage.PostOperation` | 40 | React to the completed operation (create related records, send notifications) |

The pipeline is enabled by default (`FakeOrganizationServiceOptions.EnablePipeline = true`) and is accessible through `env.Pipeline`.

```csharp
using Fake4Dataverse;
using Fake4Dataverse.Pipeline;
using Microsoft.Xrm.Sdk;
```

---

## Registering Pipeline Steps

### Lambda Callbacks

Register a lambda that receives `IPluginExecutionContext`. You can scope to all entities or a specific entity:

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();

// All entities — fires on every Create
env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
{
    var target = (Entity)ctx.InputParameters["Target"];
    target["modifiedby_custom"] = "pipeline";
});

// Entity-scoped — fires only on account Create
env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, "account", ctx =>
{
    var target = (Entity)ctx.InputParameters["Target"];
    target["accountnumber"] = "AUTO-" + Guid.NewGuid().ToString("N").Substring(0, 8);
});
```

### Convenience Methods

Shorthand helpers avoid repeating the `PipelineStage` enum:

```csharp
env.Pipeline.RegisterPreValidation("Create", ctx => { /* stage 10 */ });
env.Pipeline.RegisterPreOperation("Update", "account", ctx => { /* stage 20 */ });
env.Pipeline.RegisterPostOperation("Delete", ctx => { /* stage 40 */ });
```

### Disposable Registration

Every `RegisterStep` call returns a `PipelineStepRegistration` that implements `IDisposable`. Dispose it to unregister:

```csharp
PipelineStepRegistration reg = env.Pipeline.RegisterPostOperation("Create", ctx => { });

// Later, unregister:
reg.Dispose();
```

---

## Testing Real IPlugin Implementations

The most powerful feature: register a real `IPlugin` class and the fake service provides a complete `IServiceProvider` with:

- **`IPluginExecutionContext`** — full context with message name, entity, parameters, images
- **`IOrganizationServiceFactory`** — backed by the same `FakeOrganizationService`, so plugin CRUD calls hit the in-memory store
- **`ITracingService`** — captures trace messages readable via `env.Pipeline.Traces`

### Complete Example

Given a plugin:

```csharp
public sealed class SetAccountNumberPlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

        var target = (Entity)context.InputParameters["Target"];
        target["accountnumber"] = "ACC-" + DateTime.UtcNow.Ticks;

        tracing.Trace("Assigned account number to {0}", target.Id);
    }
}
```

Test it end-to-end:

```csharp
[Fact]
public void Create_WithPlugin_SetsAccountNumber()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    env.Pipeline.RegisterStep(
        "Create", PipelineStage.PreOperation, "account",
        new SetAccountNumberPlugin());

    var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

    var account = service.Retrieve("account", id, new ColumnSet("accountnumber"));
    Assert.StartsWith("ACC-", account.GetAttributeValue<string>("accountnumber"));
}
```

### Plugin That Creates Related Records

Plugins registered at PostOperation can call back into the service to create related records:

```csharp
[Fact]
public void CreateAccount_PluginCreatesContact()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    env.Pipeline.RegisterStep(
        "Create", PipelineStage.PostOperation, "account",
        new AccountPrimaryContactPlugin());

    var accountId = service.Create(new Entity("account") { ["name"] = "Fabrikam" });

    var contacts = service.RetrieveMultiple(new QueryExpression("contact")
    {
        ColumnSet = new ColumnSet("lastname", "parentcustomerid")
    });

    Assert.Single(contacts.Entities);
    Assert.Equal(accountId, contacts.Entities[0].GetAttributeValue<EntityReference>("parentcustomerid").Id);
}
```

---

## Pre/Post Entity Images

Images capture the entity state before and/or after the core operation. Configure them on the returned `PipelineStepRegistration`:

```csharp
var reg = env.Pipeline.RegisterStep(
    "Update", PipelineStage.PostOperation, "account",
    new AuditChangePlugin());

// Pre-image: entity state before the update (specific attributes only)
reg.AddPreImage("PreImage", "name", "revenue");

// Post-image: entity state after the update (specific attributes only)
reg.AddPostImage("PostImage", "name", "revenue");
```

### Image Attribute Filtering

Pass attribute names to limit which columns appear in the image. Only those attributes are included:

```csharp
reg.AddPreImage("NameOnly", "name");     // image contains only "name"
reg.AddPostImage("Partial", "name", "revenue"); // image contains "name" and "revenue"
```

Pass **no** attribute names to capture the full entity:

```csharp
reg.AddPreImage("FullSnapshot");  // all attributes captured
```

This matches real Dataverse plugin registration behaviour: an empty attribute list means "all
attributes"; a non-empty list restricts the image to the specified columns.

Inside plugin code, access images from the context:

```csharp
public void Execute(IServiceProvider serviceProvider)
{
    var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

    var preImage = context.PreEntityImages["PreImage"];
    var postImage = context.PostEntityImages["PostImage"];

    var oldName = preImage.GetAttributeValue<string>("name");
    var newName = postImage.GetAttributeValue<string>("name");
}
```

Fluent chaining is supported:

```csharp
env.Pipeline
    .RegisterStep("Update", PipelineStage.PostOperation, "account", new AuditChangePlugin())
    .AddPreImage("PreImage", "name", "revenue")
    .AddPostImage("PostImage", "name", "revenue");
```

---

## Execution Context Properties

`FakePipelineContext` implements `IPluginExecutionContext7`, the latest Dataverse SDK interface, which means it satisfies any plugin that resolves `IPluginExecutionContext`, `IPluginExecutionContext2`, `IPluginExecutionContext3`, `IPluginExecutionContext4`, `IPluginExecutionContext5`, `IPluginExecutionContext6`, or `IPluginExecutionContext7`.

### IExecutionContext / IPluginExecutionContext (v1)

| Property | Type | Value / Source |
|---|---|---|
| `MessageName` | `string` | Operation name: "Create", "Update", "Delete", etc. |
| `PrimaryEntityName` | `string` | Logical name of the target entity |
| `PrimaryEntityId` | `Guid` | ID of the target record |
| `InputParameters` | `ParameterCollection` | Contains "Target" (Entity or EntityReference) |
| `OutputParameters` | `ParameterCollection` | Contains "id" (Guid) after Create |
| `PreEntityImages` | `EntityImageCollection` | Pre-operation snapshots (if configured) |
| `PostEntityImages` | `EntityImageCollection` | Post-operation snapshots (if configured) |
| `Stage` | `int` | 10, 20, or 40 |
| `Depth` | `int` | Pipeline depth (always `1`) |
| `Mode` | `int` | 0 = Synchronous, 1 = Asynchronous |
| `UserId` | `Guid` | `service.CallerId` |
| `InitiatingUserId` | `Guid` | `service.InitiatingUserId` |
| `BusinessUnitId` | `Guid` | `service.BusinessUnitId` |
| `OrganizationId` | `Guid` | `env.OrganizationId` |
| `OrganizationName` | `string` | `env.OrganizationName` |
| `SharedVariables` | `ParameterCollection` | Shared across pipeline stages |
| `CorrelationId` | `Guid` | Random per-execution `Guid` |
| `OperationId` | `Guid` | Random per-execution `Guid` |
| `RequestId` | `Guid?` | Random per-execution `Guid` |
| `IsInTransaction` | `bool` | Always `true` |
| `IsolationMode` | `int` | Always `1` (Sandbox) |
| `ParentContext` | `IPluginExecutionContext` | Always `null` |

### IPluginExecutionContext2

Added in this version: Azure AD / portals context properties. Configure them on `FakeOrganizationService`.

| Property | Type | Default | Service Property |
|---|---|---|---|
| `UserAzureActiveDirectoryObjectId` | `Guid` | `Guid.Empty` | `service.UserAzureActiveDirectoryObjectId` |
| `InitiatingUserAzureActiveDirectoryObjectId` | `Guid` | `Guid.Empty` | `service.InitiatingUserAzureActiveDirectoryObjectId` |
| `InitiatingUserApplicationId` | `Guid` | `Guid.Empty` | `service.InitiatingUserApplicationId` |
| `IsPortalsClientCall` | `bool` | `false` | `service.IsPortalsClientCall` |
| `PortalsContactId` | `Guid` | `Guid.Empty` | `service.PortalsContactId` |

### IPluginExecutionContext3

| Property | Type | Default | Service Property |
|---|---|---|---|
| `AuthenticatedUserId` | `Guid` | `service.CallerId` | _(mirrors_ `CallerId` _automatically)_ |

### IPluginExecutionContext4

| Property | Type | Notes |
|---|---|---|
| `PreEntityImagesCollection` | `EntityImageCollection[]` | Single-element array wrapping `PreEntityImages` |
| `PostEntityImagesCollection` | `EntityImageCollection[]` | Single-element array wrapping `PostEntityImages` |

These are provided for plugins that use the array-of-images pattern (e.g. bulk operations). In Fake4Dataverse, which handles single-record operations, each array contains exactly one `EntityImageCollection`.

### IPluginExecutionContext5

| Property | Type | Default | Service Property |
|---|---|---|---|
| `InitiatingUserAgent` | `string` | `"Fake4Dataverse"` | `service.InitiatingUserAgent` |

### IPluginExecutionContext6

| Property | Type | Default | Service Property |
|---|---|---|---|
| `EnvironmentId` | `string` | `string.Empty` | `env.EnvironmentId` |
| `TenantId` | `Guid` | `Guid.Empty` | `env.TenantId` |

### IPluginExecutionContext7

| Property | Type | Default | Service Property |
|---|---|---|---|
| `IsApplicationUser` | `bool` | `false` | `service.IsApplicationUser` |

### Configuring v2–v7 Properties

Set session-level properties on `FakeOrganizationService` and environment-level properties on `FakeDataverseEnvironment` before running your test:

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();

// Simulate a Power Pages / portals call (session-level)
service.IsPortalsClientCall = true;
service.PortalsContactId = Guid.NewGuid();

// Provide AAD identity details (session-level)
service.UserAzureActiveDirectoryObjectId = Guid.NewGuid();
service.InitiatingUserAzureActiveDirectoryObjectId = Guid.NewGuid();
service.InitiatingUserApplicationId = Guid.NewGuid();

// Simulate a specific environment and tenant (environment-level)
env.EnvironmentId = "unq1a2b3c4d5e6f";
env.TenantId = new Guid("aaaabbbb-cccc-dddd-eeee-ffffgggghhhh");

// Simulate an application user (session-level)
service.IsApplicationUser = true;

// Set a custom caller user-agent (session-level)
service.InitiatingUserAgent = "MyIntegration/3.0";
```

Inside a plugin, cast the context to the required version:

```csharp
public void Execute(IServiceProvider serviceProvider)
{
    var ctx7 = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext7;
    if (ctx7 != null && ctx7.IsApplicationUser)
    {
        // Elevated service-principal logic
    }

    var ctx6 = ctx7 as IPluginExecutionContext6;
    // ctx6?.EnvironmentId, ctx6?.TenantId

    var ctx2 = ctx7 as IPluginExecutionContext2;
    if (ctx2?.IsPortalsClientCall == true)
    {
        // Portal-specific branching
    }
}
```

---

## Asynchronous Steps

Mark a step as asynchronous to set `Mode = 1` on the execution context:

```csharp
env.Pipeline
    .RegisterStep("Create", PipelineStage.PostOperation, "account", new AsyncNotificationPlugin())
    .SetAsynchronous();
```

The plugin can then check the mode:

```csharp
if (context.Mode == 1) // Asynchronous
{
    // async-specific logic
}
```

> **Note:** Fake4Dataverse executes asynchronous steps **synchronously in-process** regardless of
> the `Mode` setting. `SetAsynchronous()` only sets `ctx.Mode = 1` so your plugin code can branch
> on it in tests — execution order and threading are unaffected.

---

## Capturing and Asserting Plugin Traces

All calls to `ITracingService.Trace()` inside plugins are captured:

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, "account", new AccountPrimaryContactPlugin());

service.Create(new Entity("account") { ["name"] = "Contoso" });

// Read traces
Assert.Contains(env.Pipeline.Traces,
    t => t.Contains("Created primary contact for account"));

// Clear for next test
env.Pipeline.ClearTraces();
Assert.Empty(env.Pipeline.Traces);
```

---

## Auto-Registering Plugins from SPKL Attributes

The `Fake4Dataverse.Spkl` package scans your assembly for `[CrmPluginRegistration]` attributes and registers all discovered plugin steps automatically:

```
dotnet add package Fake4Dataverse.Spkl
```

```csharp
using Fake4Dataverse.Spkl;

var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();

// Register all plugins found in the assembly
var result = env.RegisterSpklPluginsFromAssembly(typeof(MyPlugin).Assembly);

// Register a single plugin type
var result2 = env.RegisterSpklPlugins(typeof(MyPlugin));
```

### SpklRegistrationResult

Both methods return a `SpklRegistrationResult` that provides details about what was registered
and what was skipped:

```csharp
// Individual PipelineStepRegistration objects with full control
IReadOnlyList<PipelineStepRegistration> regs = result.Registrations;

// Steps that could not be mapped (e.g. workflow activities, custom API steps,
// unsupported messages). Each entry has a Reason string.
IReadOnlyList<SpklSkippedRegistration> skipped = result.SkippedRegistrations;

foreach (var s in skipped)
{
    Console.WriteLine($"Skipped: {s.Attribute.Message} — {s.Reason}");
}
```

### Disposable Cleanup

`SpklRegistrationResult` implements `IDisposable`. Disposing it unregisters **all** steps that
were registered in that batch — useful for scoped plugin registration in tests:

```csharp
using (env.RegisterSpklPluginsFromAssembly(typeof(MyPlugin).Assembly))
{
    // All SPKL-registered steps are active here
    service.Create(new Entity("account") { ["name"] = "Test" });
    // assertions...
}
// All steps automatically unregistered on dispose
```

### How SPKL Attributes Map

| `[CrmPluginRegistration]` property | Pipeline registration |
|---|---|
| `Message` | `messageName` |
| `Stage` | `PipelineStage` |
| `PrimaryEntityName` | `entityName` filter |
| `FilteringAttributes` | `AddPreImage` / `AddPostImage` attribute lists |
| `ExecutionMode` (async) | `.SetAsynchronous()` |
| Image entries (`PreAndPostImages`, `PreImage`, `PostImage`) | `AddPreImage` / `AddPostImage` |

Steps that require features not supported in the fake (workflow activities, custom API steps)
are collected into `SkippedRegistrations` rather than causing an exception.

---

## Unregistering Steps

### Explicit Dispose

```csharp
var reg = env.Pipeline.RegisterPostOperation("Create", ctx => { });
// ... run tests ...
reg.Dispose(); // step is removed
```

### Using Statement

```csharp
using (env.Pipeline.RegisterPreValidation("Create", "account", ctx =>
{
    throw new InvalidPluginExecutionException("Blocked!");
}))
{
    Assert.Throws<InvalidPluginExecutionException>(
        () => service.Create(new Entity("account")));
}

// Step is unregistered — Create works again
service.Create(new Entity("account")); // succeeds
```

---

## Pipeline in Test Isolation

### Combine with Scope

`env.Scope()` snapshots and restores the **data store** on dispose, but pipeline registrations are **not** rolled back. This lets you register plugins once and test multiple data scenarios:

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, "account", new AccountPrimaryContactPlugin());

using (env.Scope())
{
    service.Create(new Entity("account") { ["name"] = "Test" });
    // contact was created by plugin
    Assert.Single(service.RetrieveMultiple(new QueryExpression("contact")).Entities);
}
// Data is rolled back — but the pipeline step is still registered

using (env.Scope())
{
    service.Create(new Entity("account") { ["name"] = "Another" });
    Assert.Single(service.RetrieveMultiple(new QueryExpression("contact")).Entities);
}
```

### Isolate Pipeline Steps per Test

If you need per-test pipeline isolation, combine `Scope()` with `using` on the registration:

```csharp
using (env.Scope())
using (env.Pipeline.RegisterPreValidation("Create", "account", ctx =>
{
    throw new InvalidPluginExecutionException("Validation failed");
}))
{
    Assert.Throws<InvalidPluginExecutionException>(
        () => service.Create(new Entity("account")));
}
// Both data AND pipeline step are cleaned up
```

---

## Tips & Patterns

**Use PreValidation for guard logic that should abort the operation:**

```csharp
env.Pipeline.RegisterPreValidation("Create", "account", ctx =>
{
    var target = (Entity)ctx.InputParameters["Target"];
    if (!target.Contains("name"))
        throw new InvalidPluginExecutionException("Account name is required.");
});
```

**Use PreOperation for field manipulation before save:**

```csharp
env.Pipeline.RegisterPreOperation("Create", "contact", ctx =>
{
    var target = (Entity)ctx.InputParameters["Target"];
    var first = target.GetAttributeValue<string>("firstname") ?? "";
    var last = target.GetAttributeValue<string>("lastname") ?? "";
    target["fullname"] = $"{first} {last}".Trim();
});
```

**Use PostOperation for side effects (create related records, update rollups):**

```csharp
env.Pipeline.RegisterPostOperation("Create", "order", ctx =>
{
    var factory = (IOrganizationServiceFactory)ctx.GetType()  // for lambda-style:
        // use the outer 'service' variable directly
        ;
    var target = (Entity)ctx.InputParameters["Target"];
    var orderId = (Guid)ctx.OutputParameters["id"];

    // For lambdas, capture the service in the closure
    service.Create(new Entity("task")
    {
        ["subject"] = "Follow up on order",
        ["regardingobjectid"] = new EntityReference("order", orderId)
    });
});
```

**Throw `InvalidPluginExecutionException` to cancel operations** — this is the standard Dataverse pattern:

```csharp
env.Pipeline.RegisterPreValidation("Delete", "account", ctx =>
{
    throw new InvalidPluginExecutionException("Accounts cannot be deleted.");
});

Assert.Throws<InvalidPluginExecutionException>(
    () => service.Delete("account", accountId));
```

**Use SharedVariables to pass data between stages:**

```csharp
env.Pipeline.RegisterPreValidation("Create", "account", ctx =>
{
    ctx.SharedVariables["ApprovedBy"] = "admin";
});

env.Pipeline.RegisterPostOperation("Create", "account", ctx =>
{
    var approver = (string)ctx.SharedVariables["ApprovedBy"];
});
```
