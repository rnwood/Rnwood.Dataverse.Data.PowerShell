# Performance & Indexing

Fake4Dataverse uses in-memory evaluation for all queries. Most test scenarios are fast by default, but large datasets can benefit from attribute indexes and configuration tuning.

---

## Attribute Indexes

Add an equality index on a frequently-queried attribute to accelerate `ConditionOperator.Equal` filters:

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();

// Create an index on account.name
env.AddIndex("account", "name");
```

### How It Works

- The index maintains a hash-table mapping attribute values to entity IDs.
- When a `QueryExpression` filters on an indexed attribute with `ConditionOperator.Equal`, the evaluator performs a hash-table lookup instead of a full scan.
- Indexes are automatically maintained on `Create`, `Update`, and `Delete`.
- Adding an index retroactively indexes all existing entities for that attribute.

### Example

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
env.AddIndex("contact", "lastname");

// Seed 10,000 contacts
for (int i = 0; i < 10_000; i++)
    service.Create(new Entity("contact") { ["lastname"] = $"Name{i}" });

// This query uses the index — fast lookup instead of scanning all 10,000 records
var query = new QueryExpression("contact");
query.Criteria.AddCondition("lastname", ConditionOperator.Equal, "Name5000");
var results = service.RetrieveMultiple(query);

Assert.Single(results.Entities);
```

---

## Query Optimization Tips

Even without indexes, you can improve query performance:

### Use Specific ColumnSet

```csharp
// Avoid: retrieves and clones all attributes
var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };

// Prefer: retrieves only what you need
var query = new QueryExpression("account")
{
    ColumnSet = new ColumnSet("name", "revenue")
};
```

### Use TopCount

```csharp
// Limit results when you only need the first N records
var query = new QueryExpression("account") { TopCount = 10 };
```

### Use Paging

```csharp
var query = new QueryExpression("account")
{
    PageInfo = new PagingInfo { PageNumber = 1, Count = 50 }
};
```

### Filter Early

Apply conditions in the `QueryExpression` rather than filtering results in your test code:

```csharp
// Prefer: filter in query
query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

// Avoid: retrieving all records then filtering in C#
var all = service.RetrieveMultiple(query);
var active = all.Entities.Where(e => e.GetAttributeValue<int>("statecode") == 0);
```

---

## Configuration for Performance

`FakeOrganizationServiceOptions` controls automatic behaviors that add overhead. Disable features you don't need:

```csharp
// Maximum performance — all automatic behaviors off
var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Lenient);
var service = env.CreateOrganizationService();
```

Or selectively disable specific features:

```csharp
var options = new FakeOrganizationServiceOptions
{
    EnablePipeline = false,        // Skip pre/post-operation hooks
    EnableOperationLog = false,    // Skip call recording
    AutoSetTimestamps = false,     // Skip createdon/modifiedon
    AutoSetOwner = false,          // Skip ownerid/createdby
    AutoSetVersionNumber = false,  // Skip versionnumber increment
    ValidateWithMetadata = false,  // Skip metadata validation
};
var env = new FakeDataverseEnvironment(options);
var service = env.CreateOrganizationService();
```

| Option                 | Impact When Disabled                        |
|------------------------|---------------------------------------------|
| `EnablePipeline`       | Skips all pre/post-operation plugin hooks    |
| `EnableOperationLog`   | Skips recording calls to `OperationLog`      |
| `AutoSetTimestamps`    | Skips `createdon` / `modifiedon` assignment  |
| `AutoSetOwner`         | Skips `ownerid` / `createdby` assignment     |
| `AutoSetVersionNumber` | Skips `versionnumber` increment              |
| `ValidateWithMetadata` | Skips attribute/entity metadata validation   |

---

## Benchmark Results

For detailed benchmark numbers, see the [Performance Benchmarks](../reference/performance.md).
