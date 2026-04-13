# Concurrency & Transactions

Fake4Dataverse replicates Dataverse's concurrency control and transactional semantics so your unit tests exercise the same behavior they would encounter in production.

## Optimistic Concurrency

Dataverse uses **optimistic concurrency** to detect conflicts when two sessions try to update or delete the same record concurrently. Each record carries a `versionnumber` (exposed via `Entity.RowVersion`) that increments on every write. A caller can demand that their write only succeeds if the record hasn't been modified since they last read it.

### How It Works

1. **Read** a record — the response includes `versionnumber`.
2. **Write** using `UpdateRequest` or `DeleteRequest` with `ConcurrencyBehavior = IfRowVersionMatches` and `RowVersion` set to the version you read.
3. If the stored version still matches, the write succeeds and the version increments.
4. If another session updated the record in the meantime, a `ConcurrencyVersionMismatch` fault is thrown.

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();

// Create a record
var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

// Read current version
var account = service.Retrieve("account", id, new ColumnSet(true));
var rowVersion = account["versionnumber"].ToString();

// Update with optimistic concurrency — succeeds
service.Execute(new UpdateRequest
{
    Target = new Entity("account", id)
    {
        ["name"] = "Contoso Ltd.",
        RowVersion = rowVersion
    },
    ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
});
```

### Handling Conflicts

When a version mismatch occurs, the SDK throws `FaultException<OrganizationServiceFault>` with error code `-2147088254` (`ConcurrencyVersionMismatch`). The typical pattern is to catch, re-read, and retry:

```csharp
var session1 = env.CreateOrganizationService();
var session2 = env.CreateOrganizationService();

var account = session1.Retrieve("account", id, new ColumnSet(true));
var version = account["versionnumber"].ToString();

// Session 1 updates first
session1.Execute(new UpdateRequest
{
    Target = new Entity("account", id) { ["name"] = "Session1", RowVersion = version },
    ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
});

// Session 2's stale version fails
try
{
    session2.Execute(new UpdateRequest
    {
        Target = new Entity("account", id) { ["name"] = "Session2", RowVersion = version },
        ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
    });
}
catch (FaultException<OrganizationServiceFault> ex)
    when (ex.Detail.ErrorCode == DataverseFault.ConcurrencyVersionMismatch)
{
    // Re-read and retry
    var fresh = session2.Retrieve("account", id, new ColumnSet(true));
    session2.Execute(new UpdateRequest
    {
        Target = new Entity("account", id)
        {
            ["name"] = "Session2-Retry",
            RowVersion = fresh["versionnumber"].ToString()
        },
        ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
    });
}
```

### ConcurrencyBehavior Values

| Value | Behavior |
|-------|----------|
| `Default` | No version check — last writer wins |
| `AlwaysOverwrite` | No version check, explicit opt-out |
| `IfRowVersionMatches` | Fails with `ConcurrencyVersionMismatch` if the stored version differs from `RowVersion` |

### Delete with Optimistic Concurrency

Works the same way via `DeleteRequest`:

```csharp
service.Execute(new DeleteRequest
{
    Target = new EntityReference("account", id) { RowVersion = rowVersion },
    ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
});
```

### Error Codes

| Code | Constant | Meaning |
|------|----------|---------|
| `-2147088254` | `DataverseFault.ConcurrencyVersionMismatch` | Row version doesn't match |
| `-2147088253` | `DataverseFault.ConcurrencyVersionNotProvided` | `IfRowVersionMatches` requested but `RowVersion` is null/empty |
| `-2147088243` | `DataverseFault.OptimisticConcurrencyNotEnabled` | Reserved for tables that don't support optimistic concurrency |

## Transactions with ExecuteTransactionRequest

`ExecuteTransactionRequest` executes a batch of requests as an atomic unit — all succeed or all fail. On failure, every change made by preceding requests in the batch is rolled back.

### Basic Usage

```csharp
var response = (ExecuteTransactionResponse)service.Execute(new ExecuteTransactionRequest
{
    Requests = new OrganizationRequestCollection
    {
        new CreateRequest { Target = new Entity("account") { ["name"] = "A" } },
        new CreateRequest { Target = new Entity("account") { ["name"] = "B" } },
        new CreateRequest { Target = new Entity("account") { ["name"] = "C" } }
    },
    ReturnResponses = true
});

// All 3 accounts created; response.Responses has 3 entries
```

### Rollback on Failure

If the third request fails, the first two are rolled back:

```csharp
var existingId = Guid.NewGuid();
service.Create(new Entity("account", existingId) { ["name"] = "Existing" });

Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
    service.Execute(new ExecuteTransactionRequest
    {
        Requests = new OrganizationRequestCollection
        {
            new CreateRequest { Target = new Entity("account") { ["name"] = "New1" } },
            new CreateRequest { Target = new Entity("account") { ["name"] = "New2" } },
            // Duplicate ID — fails
            new CreateRequest { Target = new Entity("account", existingId) { ["name"] = "Dup" } }
        }
    }));

// New1 and New2 were rolled back — only "Existing" remains
var all = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
Assert.Single(all.Entities);
```

### How Rollback Works

Fake4Dataverse uses a **copy-on-write (CoW) strategy** rather than full environment snapshots or undo logs. When a transaction begins, a `TransactionCopyOnWriteState` is attached to the store. All writes during the transaction are staged in this CoW state; the shared store is only mutated when the transaction commits.

| Operation | On Commit | On Rollback |
|-----------|-----------|-------------|
| Create | Record written to shared store | Staged record discarded |
| Update | Updated record written to shared store | Staged update discarded |
| Delete | Record removed from shared store | Staged deletion discarded |

On failure, the staged CoW state is simply discarded — no inverse operations are needed. This approach is **concurrency-safe**: rolling back one transaction only discards that transaction's staged changes, leaving the shared store untouched.

### Nesting Rules

Matching real Dataverse behavior:

- `ExecuteMultipleRequest` **can contain** `ExecuteTransactionRequest`
- `ExecuteTransactionRequest` **cannot contain** `ExecuteMultipleRequest`
- `ExecuteTransactionRequest` **cannot contain** `ExecuteTransactionRequest`

```csharp
// This is valid — ExecuteMultiple wrapping ExecuteTransaction
service.Execute(new ExecuteMultipleRequest
{
    Requests = new OrganizationRequestCollection
    {
        new ExecuteTransactionRequest
        {
            Requests = new OrganizationRequestCollection { /* ... */ },
            ReturnResponses = true
        }
    },
    Settings = new ExecuteMultipleSettings { ContinueOnError = false, ReturnResponses = true }
});
```

### Combining Transactions with Optimistic Concurrency

You can use `ConcurrencyBehavior.IfRowVersionMatches` inside a transaction. If the concurrency check fails, the entire transaction rolls back:

```csharp
var requests = new OrganizationRequestCollection
{
    new UpdateRequest { Target = new Entity("account", id1) { ["name"] = "Updated1" } },
    new UpdateRequest
    {
        Target = new Entity("account", id2) { ["name"] = "Updated2", RowVersion = staleVersion },
        ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
    }
};

// Second request fails → first request's update is rolled back
Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
    service.Execute(new ExecuteTransactionRequest { Requests = requests }));
```

## Implicit Transaction Scopes

In real Dataverse, every top-level operation (`Create`, `Update`, `Delete`) runs inside an implicit database transaction. The pipeline stages execute in this order:

1. **PreValidation** — runs *outside* the transaction
2. **PreOperation** → **Core operation** → **PostOperation** — run *inside* the transaction

If a **PostOperation** plugin throws an exception, the entire core mutation is rolled back — the record is restored to its state before the operation began. Fake4Dataverse replicates this behavior using the same copy-on-write mechanism used by `ExecuteTransactionRequest`.

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();

// Create a record
var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

// Register a PostOperation plugin that throws
env.Pipeline.RegisterPostOperation("Update", "account", ctx =>
{
    throw new InvalidPluginExecutionException("Audit service unavailable");
});

// The update throws, but the original value is preserved
Assert.Throws<InvalidPluginExecutionException>(() =>
    service.Update(new Entity("account", id) { ["name"] = "Modified" }));

var account = service.Retrieve("account", id, new ColumnSet(true));
Assert.Equal("Contoso", account.GetAttributeValue<string>("name")); // Rolled back
```

This also applies to `Create` (the entity is removed on rollback) and `Delete` (the entity is restored on rollback).

### Nested Transactions

When an operation runs inside an `ExecuteTransactionRequest`, the implicit transaction scope detects the outer transaction and reuses it — no nested CoW transaction state is created. A failure still propagates to the outer transaction which handles rollback of all operations in the batch.

## Version Numbers

The `versionnumber` attribute is a monotonically increasing `long` managed by `FakeDataverseEnvironment`:

- Automatically set on Create and Update when `AutoSetVersionNumber = true` (the default)
- Uses `Interlocked.Increment` for thread-safe atomic increments
- Version numbers are **not** rolled back when transactions fail (matching SQL Server behavior where sequence values are consumed even on rollback)
- Disable with `new FakeOrganizationServiceOptions { AutoSetVersionNumber = false }`

## Configuration

All concurrency features are enabled by default:

| Option | Default | Effect |
|--------|---------|--------|
| `AutoSetVersionNumber` | `true` | Increments `versionnumber` on Create/Update |

The `Strict` preset includes version numbering. The `Lenient` preset disables it along with all other auto-set behaviors.

## Comparison with Real Dataverse

| Feature | Real Dataverse | Fake4Dataverse |
|---------|---------------|----------------|
| Optimistic concurrency (`IfRowVersionMatches`) | SQL-level row version check | In-memory version check |
| Transaction rollback | SQL transaction rollback | CoW state discard |
| Implicit transaction on CRUD | SQL transaction wrapping pipeline | CoW transaction wrapping pipeline |
| Concurrent transaction isolation | SQL Server locking | CoW state per-transaction |
| Version number generation | SQL row version | `Interlocked.Increment` |
| Version numbers on rollback | Not reclaimed | Not reclaimed |
| `ExecuteTransaction` nesting validation | Enforced | Enforced |
