# Configuration Reference

## FakeOrganizationServiceOptions

All behavior of `FakeDataverseEnvironment` is controlled through `FakeOrganizationServiceOptions`, passed at construction time.

### Presets

| Preset | Description |
|--------|-------------|
| **Default** (`new FakeOrganizationServiceOptions()`) | All auto-set behaviors ON, metadata validation and security OFF |
| **Strict** (`FakeOrganizationServiceOptions.Strict`) | Metadata validation + security enforcement ON (all auto-set behaviors remain ON) |
| **Lenient** (`FakeOrganizationServiceOptions.Lenient`) | All features OFF — maximum speed, full manual control |

```csharp
// Default — recommended starting point
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();

// Strict — mirrors real Dataverse validation
var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Strict);
var service = env.CreateOrganizationService();

// Lenient — bare-bones, nothing automatic
var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Lenient);
var service = env.CreateOrganizationService();
```

### All Options

| Option | Type | Default | Strict | Lenient | Description |
|--------|------|---------|--------|---------|-------------|
| `AutoSetTimestamps` | `bool` | `true` | `true` | `false` | Auto-set `createdon`/`modifiedon` on Create and Update |
| `AutoSetOwner` | `bool` | `true` | `true` | `false` | Auto-set `ownerid`, `createdby`, `modifiedby` from `CallerId` |
| `AutoSetVersionNumber` | `bool` | `true` | `true` | `false` | Auto-increment `versionnumber` on Create/Update |
| `AutoSetStateCode` | `bool` | `true` | `true` | `false` | Auto-set `statecode=0`/`statuscode=1` on Create |
| `ValidateWithMetadata` | `bool` | `false` | `true` | `false` | Requires entity metadata registration; validates on Create/Update |
| `EnforceSecurityRoles` | `bool` | `false` | `true` | `false` | Enforces security role/privilege checks |
| `EnablePipeline` | `bool` | `true` | `true` | `false` | Enables pre/post operation pipeline hooks |
| `EnableOperationLog` | `bool` | `true` | `true` | `false` | Records all operations for assertions |

### Loading Options

**Programmatic:**

```csharp
var options = new FakeOrganizationServiceOptions
{
    AutoSetTimestamps = true,
    ValidateWithMetadata = true,
    EnforceSecurityRoles = false,
};
var env = new FakeDataverseEnvironment(options);
var service = env.CreateOrganizationService();
```

**From a JSON string (`FromJson`):**

```csharp
var json = """{"AutoSetTimestamps": false, "ValidateWithMetadata": true}""";
var options = FakeOrganizationServiceOptions.FromJson(json);
var env = new FakeDataverseEnvironment(options);
var service = env.CreateOrganizationService();
```

Unspecified properties keep their default values. Property names are case-insensitive.

**From a JSON file (`FromJsonFile`):**

```csharp
var options = FakeOrganizationServiceOptions.FromJsonFile("fake4dataverse.json");
var env = new FakeDataverseEnvironment(options);
var service = env.CreateOrganizationService();
```

**From environment variables (`FromEnvironment`):**

```csharp
// Reads FAKE4DATAVERSE_AUTOSETTIMESTAMPS, FAKE4DATAVERSE_VALIDATEWITHMETADATA, etc.
var options = FakeOrganizationServiceOptions.FromEnvironment();
var env = new FakeDataverseEnvironment(options);
var service = env.CreateOrganizationService();
```

Environment variable names are `FAKE4DATAVERSE_` followed by the option name in uppercase. Only variables that are set override the defaults.

---

## Properties

Properties are split between `FakeDataverseEnvironment` (shared state) and `FakeOrganizationService` (per-session state).

### Environment Properties (`FakeDataverseEnvironment`)

| Property | Type | Description |
|----------|------|-------------|
| `Options` | `FakeOrganizationServiceOptions` | Read-only configuration options |
| `Clock` | `IClock` | Clock implementation (default: `SystemClock`) |
| `OrganizationId` | `Guid` | Organization identifier |
| `OrganizationName` | `string` | Organization name (surfaced to plugins) |
| `EnvironmentId` | `string` | Simulated environment ID |
| `TenantId` | `Guid` | Simulated tenant ID |
| `MetadataStore` | `InMemoryMetadataStore` | Entity/attribute metadata store |
| `Security` | `SecurityManager` | Security roles, privileges, and record sharing |
| `Pipeline` | `PipelineManager` | Pre/post operation pipeline hooks |
| `CalculatedFields` | `CalculatedFieldManager` | Calculated and rollup field evaluation |
| `Currency` | `CurrencyManager` | Exchange rates and base currency computation |
| `OperationLog` | `OperationLog` | Global recorded operations for assertions |
| `HandlerRegistry` | `OrganizationRequestHandlerRegistry` | Registry for custom request handlers |

### Session Properties (`FakeOrganizationService`)

| Property | Type | Description |
|----------|------|-------------|
| `CallerId` | `Guid` | Current user ID (used for auto-set owner and security checks) |
| `InitiatingUserId` | `Guid` | Initiating user (differs from `CallerId` in impersonation scenarios) |
| `BusinessUnitId` | `Guid` | Current user's business unit |
| `UseSystemContext` | `bool` | When `true`, all security checks are bypassed |
| `OperationLog` | `OperationLog` | Per-session recorded operations for assertions |

```csharp
var env = new FakeDataverseEnvironment();
env.Clock = new FakeClock(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

var service = env.CreateOrganizationService();
service.CallerId = new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
```

---

## Auto-Set Behaviors Detail

### AutoSetTimestamps

On **Create**: sets `createdon` and `modifiedon` to `Clock.UtcNow`.
On **Update**: sets `modifiedon` to `Clock.UtcNow`.

Use `FakeClock` to control the time deterministically in tests:

```csharp
var clock = new FakeClock(new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc));
var env = new FakeDataverseEnvironment();
env.Clock = clock;
var service = env.CreateOrganizationService();

var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
var entity = service.Retrieve("account", id, new ColumnSet(true));

Assert.Equal(clock.UtcNow, entity.GetAttributeValue<DateTime>("createdon"));
```

### AutoSetOwner

On **Create**: sets `ownerid`, `createdby`, and `modifiedby` to `CallerId`.
On **Update**: sets `modifiedby` to `CallerId`.

```csharp
service.CallerId = userId;
var id = service.Create(new Entity("account") { ["name"] = "Test" });
var entity = service.Retrieve("account", id, new ColumnSet(true));

Assert.Equal(userId, entity.GetAttributeValue<EntityReference>("ownerid").Id);
Assert.Equal(userId, entity.GetAttributeValue<EntityReference>("createdby").Id);
```

### AutoSetVersionNumber

On **Create**: sets `versionnumber` to the next global counter value.
On **Update**: increments `versionnumber` to the next global counter value.

The version number is a global counter across all entities in the store, incremented atomically via `Interlocked.Increment`. This value is used by **optimistic concurrency** — when `UpdateRequest.ConcurrencyBehavior` or `DeleteRequest.ConcurrencyBehavior` is set to `IfRowVersionMatches`, the stored version must match the request's `RowVersion` or a `ConcurrencyVersionMismatch` fault is thrown.

See [Concurrency & Transactions](../guides/concurrency-transactions.md) for details.

### AutoSetStateCode

On **Create**: sets `statecode` to `0` (Active) and `statuscode` to `1` (Active).

These values match the default Dataverse behavior for most entities. Use `SetStateRequest` or `UpdateRequest` to change state after creation.

---

## See Also

- [Getting Started](../guides/getting-started.md)
- [Concurrency & Transactions](../guides/concurrency-transactions.md)
- [Metadata Validation](../guides/metadata-validation.md)
- [Security & Access Control](../guides/security-access-control.md)
- [Pipeline & Plugin Testing](../guides/pipeline-plugin-testing.md)
