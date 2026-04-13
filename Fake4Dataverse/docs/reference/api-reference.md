# API Quick Reference

Curated summary of the Fake4Dataverse public API surface.

---

## FakeDataverseEnvironment

Shared backend — owns the store, metadata, pipeline, security, clock, and configuration.

### Constructors

```csharp
FakeDataverseEnvironment()
FakeDataverseEnvironment(FakeOrganizationServiceOptions options)
```

### Session Creation

```csharp
FakeOrganizationService CreateOrganizationService()
FakeOrganizationService CreateOrganizationService(Guid callerId)
```

### Properties

| Property | Type | Description |
|---|---|---|
| `Options` | `FakeOrganizationServiceOptions` | Configuration snapshot passed at construction |
| `MetadataStore` | `InMemoryMetadataStore` | Entity/attribute metadata registry |
| `Security` | `SecurityManager` | Roles, privileges, and record sharing |
| `Pipeline` | `PipelineManager` | Pre/post-operation plugin hooks |
| `CalculatedFields` | `CalculatedFieldManager` | Calculated and rollup field evaluation |
| `Currency` | `CurrencyManager` | Exchange rates and base currency |
| `OperationLog` | `OperationLog` | Global recorded service calls for assertions |
| `HandlerRegistry` | `OrganizationRequestHandlerRegistry` | Custom request handler registration |
| `Clock` | `IClock` | Time provider (swap in `FakeClock` for deterministic tests) |
| `OrganizationId` | `Guid` | Simulated organization |
| `OrganizationName` | `string` | Simulated organization name |
| `EnvironmentId` | `string` | Simulated environment ID |
| `TenantId` | `Guid` | Simulated tenant ID |

### Seeding

```csharp
void Seed(params Entity[] entities)
void Seed(IEnumerable<Entity> entities)
void SeedFromJson(string json)
void SeedFromCsv(string csv)
```

### Binary Operations

```csharp
void SetBinaryAttribute(string entityName, Guid entityId, string attributeName, byte[] data)
byte[]? GetBinaryAttribute(string entityName, Guid entityId, string attributeName)
bool RemoveBinaryAttribute(string entityName, Guid entityId, string attributeName)  // true if removed
```

### Snapshot / Scope

```csharp
object TakeSnapshot()
void RestoreSnapshot(object snapshot)
IDisposable Scope()   // auto-restores on Dispose
```

### Time

```csharp
void AdvanceTime(TimeSpan duration)
```

### Custom API Registration

```csharp
void RegisterCustomApi(string requestName, Func<OrganizationRequest, IOrganizationService, OrganizationResponse> handler)
```

### Status Transitions

```csharp
void RegisterStatusTransition(string entityName, int fromStateCode, int fromStatusCode, int toStateCode, int toStatusCode)
void RegisterDefaultStatusCode(string entityName, int stateCode, int statusCode)
```

### Saved Queries

```csharp
EntityCollection ExecuteSavedQuery(Guid queryId)
```

### Indexing

```csharp
void AddIndex(string entityName, string attributeName)
```

### Reset

```csharp
void Reset()   // clears all entity data and binary data; does not clear metadata, pipeline, or security
```

### Solution-Aware Entities

```csharp
void RegisterSolutionAwareEntity(string entityName)
bool IsSolutionAwareEntity(string entityName)
```

Solution-aware entities are staged in an unpublished store on `Create`/`Update` and are
promoted to the main store by `PublishXmlRequest` or `PublishAllXmlRequest`. Normal
`Retrieve` and `RetrieveMultiple` return only published records.
See the [Solution-Aware Entities guide](../guides/solution-aware-entities.md).

---

## FakeOrganizationService

Lightweight session — implements both `IOrganizationService` and `IOrganizationServiceAsync2`. Created via `env.CreateOrganizationService()`.

### IOrganizationService Methods

```csharp
Guid Create(Entity entity)
Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
void Update(Entity entity)
void Delete(string entityName, Guid id)
EntityCollection RetrieveMultiple(QueryBase query)
OrganizationResponse Execute(OrganizationRequest request)
void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
```

### IOrganizationServiceAsync2 Methods

```csharp
Task<Guid> CreateAsync(Entity entity, CancellationToken cancellationToken)
Task<Entity> CreateAndReturnAsync(Entity entity, CancellationToken cancellationToken)
Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columnSet, CancellationToken cancellationToken)
Task<EntityCollection> RetrieveMultipleAsync(QueryBase query, CancellationToken cancellationToken)
Task UpdateAsync(Entity entity, CancellationToken cancellationToken)
Task DeleteAsync(string entityName, Guid id, CancellationToken cancellationToken)
Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request, CancellationToken cancellationToken)
Task AssociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities, CancellationToken cancellationToken)
Task DisassociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities, CancellationToken cancellationToken)
```

### Properties

| Property | Type | Description |
|---|---|---|
| `CallerId` | `Guid` | Simulated calling user |
| `InitiatingUserId` | `Guid` | Simulated initiating user |
| `BusinessUnitId` | `Guid` | Simulated business unit |
| `UseSystemContext` | `bool` | Bypass security checks |
| `OperationLog` | `OperationLog` | Per-session recorded service calls for assertions |
| `Environment` | `FakeDataverseEnvironment` | The environment that owns this session |

### Seeding

_Seeding, binary, snapshot, time, custom API, status transition, indexing, and reset methods are on `FakeDataverseEnvironment` (see above)._

---

## FakeOrganizationServiceOptions

All properties are `bool`. Defaults shown.

| Property | Default | Description |
|---|---|---|
| `AutoSetTimestamps` | `true` | Sets `createdon`/`modifiedon` |
| `AutoSetOwner` | `true` | Sets `ownerid`, `createdby`, `modifiedby` |
| `AutoSetVersionNumber` | `true` | Increments `versionnumber` |
| `AutoSetStateCode` | `true` | Sets `statecode`/`statuscode` on Create |
| `ValidateWithMetadata` | `false` | Requires metadata registration |
| `EnforceSecurityRoles` | `false` | Requires role setup |
| `EnablePipeline` | `true` | Plugin-like hooks |
| `EnableOperationLog` | `true` | Call recording |

### Static Presets

```csharp
static FakeOrganizationServiceOptions Strict   // metadata + security on
static FakeOrganizationServiceOptions Lenient  // all features off
```

### Factory Methods

```csharp
static FakeOrganizationServiceOptions FromJson(string json)
static FakeOrganizationServiceOptions FromJsonFile(string path)
static FakeOrganizationServiceOptions FromEnvironment()
```

---

## InMemoryMetadataStore

```csharp
bool AutoDiscoverMetadata { get; set; }
EntityMetadataBuilder AddEntity(string logicalName)
void AddOneToManyRelationship(string referencingEntity, string referencingAttribute, string referencedEntity, string referencedAttribute)
void AddOneToManyRelationship(string schemaName, string referencingEntity, string referencingAttribute, string referencedEntity, string referencedAttribute)
void AddManyToManyRelationship(string relationshipSchemaName, string entity1, string entity2, string intersectEntity)
```

---

## EntityMetadataBuilder

Fluent builder returned by `MetadataStore.AddEntity()`. All methods return `EntityMetadataBuilder`.

```csharp
WithSchemaName(string schemaName)
WithPrimaryIdAttribute(string attributeName)
WithPrimaryNameAttribute(string attributeName)
WithObjectTypeCode(int code)
WithAttribute(string logicalName, AttributeTypeCode type, ...)
WithStringAttribute(string logicalName, int maxLength, ...)
WithIntegerAttribute(string logicalName, int min, int max, ...)
WithDecimalAttribute(string logicalName, decimal min, decimal max, ...)
WithDoubleAttribute(string logicalName, double min, double max, ...)
WithMoneyAttribute(string logicalName, decimal min, decimal max, ...)
WithBooleanAttribute(string logicalName, ...)
WithDateTimeAttribute(string logicalName, ...)
WithOptionSetAttribute(string logicalName, params (int value, string label)[] options)
WithLookupAttribute(string logicalName, string targetEntity, ...)
WithOneToManyRelationship(string schemaName, string referencingEntity, string referencingAttribute)
WithManyToManyRelationship(string schemaName, string targetEntity, string intersectEntity)
WithAlternateKey(string name, params string[] attributeNames)
```

---

## PipelineManager

```csharp
IReadOnlyList<string> Traces { get; }
void ClearTraces()
```

### RegisterStep Overloads

```csharp
PipelineStepRegistration RegisterStep(string messageName, PipelineStage stage, Action<IPluginExecutionContext> callback)
PipelineStepRegistration RegisterStep(string messageName, PipelineStage stage, string? entityName, Action<IPluginExecutionContext> callback)
PipelineStepRegistration RegisterStep(IPlugin plugin, string messageName, PipelineStage stage, string? entityName)
PipelineStepRegistration RegisterStep(Type pluginType, string messageName, PipelineStage stage, string? entityName)
```

### Convenience Methods

```csharp
PipelineStepRegistration RegisterPreValidation(string messageName, Action<IPluginExecutionContext> callback)
PipelineStepRegistration RegisterPreValidation(string messageName, string? entityName, Action<IPluginExecutionContext> callback)
PipelineStepRegistration RegisterPreOperation(string messageName, Action<IPluginExecutionContext> callback)
PipelineStepRegistration RegisterPreOperation(string messageName, string? entityName, Action<IPluginExecutionContext> callback)
PipelineStepRegistration RegisterPostOperation(string messageName, Action<IPluginExecutionContext> callback)
PipelineStepRegistration RegisterPostOperation(string messageName, string? entityName, Action<IPluginExecutionContext> callback)
```

---

## PipelineStage

```csharp
enum PipelineStage
{
    PreValidation  = 10,
    PreOperation   = 20,
    PostOperation  = 40
}
```

---

## PipelineStepRegistration

Returned by `RegisterStep`. Implements `IDisposable` (unregisters on dispose).

```csharp
int Mode { get; set; }
PipelineStepRegistration AddPreImage(string imageName, params string[] attributes)
PipelineStepRegistration AddPostImage(string imageName, params string[] attributes)
PipelineStepRegistration SetAsynchronous()
void Dispose()
```

---

## FakePipelineContext

Implements `IPluginExecutionContext`. Key properties:

```csharp
string MessageName { get; }
string PrimaryEntityName { get; }
Guid PrimaryEntityId { get; set; }
ParameterCollection InputParameters { get; }
ParameterCollection OutputParameters { get; }
EntityImageCollection PreEntityImages { get; }
EntityImageCollection PostEntityImages { get; }
int Stage { get; }
PipelineStage PipelineStage { get; }
int Depth { get; }
Guid UserId { get; }
Guid InitiatingUserId { get; }
Guid BusinessUnitId { get; }
Guid OrganizationId { get; }
string OrganizationName { get; }
ParameterCollection SharedVariables { get; }
Guid CorrelationId { get; }
Guid OperationId { get; }
DateTime OperationCreatedOn { get; }
Guid? RequestId { get; }
```

---

## SecurityManager

```csharp
// Error code constant
const int PrivilegeDepthNotSatisfied  // 0x80040220 — thrown on privilege enforcement failures

bool EnforceSecurityRoles { get; set; }

// Role management
void AssignRole(Guid userId, SecurityRole role)
void ClearRoles(Guid userId)
void CheckPrivilege(Guid userId, string entityName, PrivilegeType privilege)
void CheckRecordPrivilege(Guid userId, string entityName, Guid entityId, PrivilegeType privilege, Guid? ownerId)

// Record sharing
void GrantAccess(string entityName, Guid entityId, Guid principalId, AccessRights rights)
void ModifyAccess(string entityName, Guid entityId, Guid principalId, AccessRights rights)
void RevokeAccess(string entityName, Guid entityId, Guid principalId)
AccessRights RetrievePrincipalAccess(string entityName, Guid entityId, Guid principalId, Guid? ownerId)

// Teams
void AddTeamMember(Guid teamId, Guid userId)
void RemoveTeamMember(Guid teamId, Guid userId)
void AssignTeamRole(Guid teamId, SecurityRole role)
void GrantTeamAccess(string entityName, Guid entityId, Guid teamId, AccessRights rights)
```

---

## SecurityRole

```csharp
SecurityRole(string name)
string Name { get; }
SecurityRole AddPrivilege(string entityName, PrivilegeType privilege, PrivilegeDepth depth)
PrivilegeDepth GetDepth(string entityName, PrivilegeType privilege)
```

---

## PrivilegeType

```csharp
enum PrivilegeType { Create, Read, Write, Delete, Append, AppendTo, Share, Assign }
```

## PrivilegeDepth

```csharp
enum PrivilegeDepth { None = 0, User = 1, BusinessUnit = 2, ParentChildBusinessUnit = 3, Organization = 4 }
```

---

## CalculatedFieldManager

```csharp
void RegisterCalculatedField(string entityName, string attributeName, Func<Entity, object?> formula)
void RegisterRollupField(string entityName, string attributeName, string relatedEntity, string relatedAttribute, string lookupAttribute, RollupType aggregateType, FilterExpression? filter = null)
```

## RollupType

```csharp
enum RollupType { Sum, Count, Avg, Min, Max }
```

---

## CurrencyManager

```csharp
Guid BaseCurrencyId { get; set; }
void SetExchangeRate(Guid currencyId, decimal rate)
decimal GetExchangeRate(Guid currencyId)
```

---

## IClock / FakeClock / SystemClock

```csharp
// IClock
DateTime UtcNow { get; }

// FakeClock
FakeClock()                      // defaults to 2026-01-01 UTC
FakeClock(DateTime utcNow)
void Advance(TimeSpan duration)  // relative forward step
void Set(DateTime utcNow)        // jump to an absolute time, mutates in-place

// SystemClock
static readonly SystemClock Instance
```

---

## OperationLog

```csharp
IReadOnlyList<OperationRecord> Records { get; }
void Clear()
bool HasCreated(string entityName)
bool HasCreated(string entityName, Guid id)
bool HasUpdated(string entityName, Guid id)
bool HasDeleted(string entityName, Guid id)
bool HasExecuted<TRequest>() where TRequest : OrganizationRequest
bool HasExecuted(string requestName)
IReadOnlyList<OperationRecord> GetOperations(string operationType)
IReadOnlyList<OperationRecord> GetOperations(string operationType, string entityName)
```

## OperationRecord

```csharp
string OperationType { get; }
string? EntityName { get; }
Guid? EntityId { get; }
DateTime Timestamp { get; }
Entity? Entity { get; }
OrganizationRequest? Request { get; }
```

---

## FakeOrganizationServiceAssertions

Accessed via `service.Should()` extension method. All assertion methods return `FakeOrganizationServiceAssertions` for chaining.

```csharp
FakeOrganizationServiceAssertions Should(this FakeOrganizationService service)

HaveCreated(string entityName)
HaveCreated(string entityName, Guid id)
HaveUpdated(string entityName, Guid id)
HaveUpdated(string entityName, Guid id, params (string attributeName, object? expectedValue)[] attrs)
HaveDeleted(string entityName, Guid id)
HaveExecuted<TRequest>() where TRequest : OrganizationRequest
HaveExecuted(string requestName)
NotHaveCreated(string entityName)
NotHaveDeleted(string entityName, Guid id)
NotHaveExecuted<TRequest>() where TRequest : OrganizationRequest
```

---

## EntityBuilder

Fluent builder for constructing `Entity` instances.

```csharp
EntityBuilder(string entityName)
EntityBuilder WithId(Guid id)
EntityBuilder WithAttribute(string name, object value)
EntityBuilder WithName(string name)
EntityBuilder WithState(int statecode, int statuscode = 1)
EntityBuilder WithOwner(Guid ownerId)
Entity Build()
```

---

## DataProviderExtensions

Extension methods on `FakeOrganizationService`:

```csharp
void SeedFromJsonFile(string filePath)
void SeedFromJsonString(string json)
void SeedFromCsvFile(string filePath)
```

---

## DataverseFault

Static factory for Dataverse-style `FaultException<OrganizationServiceFault>`.

```csharp
// Error code constants
const int ObjectDoesNotExist             // 0x80040217
const int DuplicateRecord                // 0x80040237
const int InvalidArgument               // 0x80040203
const int Unspecified                   // 0x80040216
const int ConcurrencyVersionMismatch    // 0x80060882
const int ConcurrencyVersionNotProvided // 0x80060883
const int OptimisticConcurrencyNotEnabled // 0x80060893

// Factory methods
static FaultException<OrganizationServiceFault> EntityNotFound(string entityName, Guid id)
static FaultException<OrganizationServiceFault> DuplicateId(string entityName, Guid id)
static FaultException<OrganizationServiceFault> InvalidArgumentFault(string message)
static FaultException<OrganizationServiceFault> ConcurrencyVersionMismatchFault(string entityName, Guid id)
static FaultException<OrganizationServiceFault> ConcurrencyVersionNotProvidedFault()
static FaultException<OrganizationServiceFault> Create(int errorCode, string message)
```

---

## EarlyBound Extensions

Extension methods on `FakeOrganizationService`:

```csharp
void RegisterEarlyBoundEntities(Assembly assembly)
void RegisterEarlyBoundEntity<TEntity>() where TEntity : Entity
```

---

## IOrganizationRequestHandler

Implement this interface to add support for a new request type.

```csharp
bool CanHandle(OrganizationRequest request)
OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
```

## OrganizationRequestHandlerRegistry

```csharp
void Register(IOrganizationRequestHandler handler)
```
