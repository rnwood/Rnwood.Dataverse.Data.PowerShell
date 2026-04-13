# Querying with QueryExpression & FetchXml

Fake4Dataverse evaluates queries entirely in memory — no SQL database, no live Dataverse connection.
This guide covers the supported in-memory query surface.

## Overview

`service.RetrieveMultiple(QueryBase query)` accepts three query types:

| Type | Use case |
|---|---|
| `QueryExpression` | Strongly-typed, most common in C# |
| `FetchExpression` | XML-based, supports aggregation |
| `QueryByAttribute` | Simple equality filters |

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();

// Seed data
service.Create(new Entity("account") { ["name"] = "Contoso", ["revenue"] = new Money(500000m) });
service.Create(new Entity("account") { ["name"] = "Fabrikam", ["revenue"] = new Money(200000m) });
```

---

## QueryExpression Basics

### ColumnSet

```csharp
// Specific columns (recommended)
var query = new QueryExpression("account")
{
    ColumnSet = new ColumnSet("name", "revenue")
};

// All columns (convenient but less performant in production)
var query = new QueryExpression("account")
{
    ColumnSet = new ColumnSet(true)
};
```

### Criteria & Conditions

```csharp
var query = new QueryExpression("account")
{
    ColumnSet = new ColumnSet("name", "revenue")
};
query.Criteria.AddCondition("revenue", ConditionOperator.GreaterThan, new Money(300000m));

EntityCollection results = service.RetrieveMultiple(query);
// results.Entities → Contoso only
```

### Ordering

```csharp
var query = new QueryExpression("account")
{
    ColumnSet = new ColumnSet("name")
};
query.AddOrder("name", OrderType.Ascending);
```

---

## Condition Operators

Fake4Dataverse supports 40+ `ConditionOperator` values, grouped below.

### Comparison

`Equal`, `NotEqual`, `Null`, `NotNull`, `GreaterThan`, `GreaterEqual`, `LessThan`, `LessEqual`, `Between`, `NotBetween`

```csharp
// Equal
query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

// Between (inclusive of both bounds)
query.Criteria.AddCondition("revenue", ConditionOperator.Between, new Money(100000m), new Money(600000m));

// Null check
query.Criteria.AddCondition("emailaddress1", ConditionOperator.NotNull);
```

### String

`Like`, `NotLike`, `BeginsWith`, `EndsWith`, `DoesNotBeginWith`, `DoesNotEndWith`, `Contains`, `DoesNotContain`

```csharp
// Wildcard search — % is the wildcard
query.Criteria.AddCondition("name", ConditionOperator.Like, "Cont%");

// Contains (substring match)
query.Criteria.AddCondition("name", ConditionOperator.Contains, "oso");
```

### Collections

`In`, `NotIn`, `ContainValues`, `DoesNotContainValues`

```csharp
// In — match any value in the list
query.Criteria.AddCondition("statuscode", ConditionOperator.In, 1, 2, 3);

// ContainValues / DoesNotContainValues — for multi-select option sets
query.Criteria.AddCondition("industries", ConditionOperator.ContainValues, 100, 200);
```

### Date — Day-Level

`Yesterday`, `Today`, `Tomorrow`

### Date — Relative Periods

`Last7Days`, `Next7Days`, `LastWeek`, `ThisWeek`, `NextWeek`, `LastMonth`, `ThisMonth`, `NextMonth`, `LastYear`, `ThisYear`, `NextYear`

### Date — X Ranges

`LastXHours`, `NextXHours`, `LastXDays`, `NextXDays`, `LastXWeeks`, `NextXWeeks`, `LastXMonths`, `NextXMonths`, `LastXYears`, `NextXYears`

```csharp
// Records created in the last 30 days
query.Criteria.AddCondition("createdon", ConditionOperator.LastXDays, 30);
```

### Date — Absolute

`On`, `OnOrBefore`, `OnOrAfter`

### Date — Age

`OlderThanXMinutes`, `OlderThanXHours`, `OlderThanXDays`, `OlderThanXWeeks`, `OlderThanXMonths`, `OlderThanXYears`

### User / Business Unit Context

`EqualUserId`, `NotEqualUserId`, `EqualBusinessId`, `NotEqualBusinessId`

```csharp
// Records owned by the calling user
query.Criteria.AddCondition("ownerid", ConditionOperator.EqualUserId);
```

> **Tip:** Use `FakeClock` for deterministic date/time operator tests — it controls the "now" value the evaluator uses.

---

## LinkEntity (Joins)

### Inner Join

```csharp
var contactId = service.Create(new Entity("contact") { ["fullname"] = "John Smith" });
service.Create(new Entity("account")
{
    ["name"] = "Contoso",
    ["primarycontactid"] = new EntityReference("contact", contactId)
});

var query = new QueryExpression("account")
{
    ColumnSet = new ColumnSet("name")
};
var link = query.AddLink("contact", "primarycontactid", "contactid", JoinOperator.Inner);
link.EntityAlias = "c";
link.Columns = new ColumnSet("fullname");

var results = service.RetrieveMultiple(query);

// Access linked column via AliasedValue
var fullname = results.Entities[0].GetAttributeValue<AliasedValue>("c.fullname")?.Value;
// fullname → "John Smith"
```

### Left Outer Join

Returns the parent row even when no matching linked row exists.

```csharp
var link = query.AddLink("contact", "primarycontactid", "contactid", JoinOperator.LeftOuter);
```

### Nested Links (Multi-Level Joins)

Chain link entities for multi-hop relationships.

```csharp
var query = new QueryExpression("opportunity") { ColumnSet = new ColumnSet("name") };
var accountLink = query.AddLink("account", "parentaccountid", "accountid");
accountLink.EntityAlias = "a";

// Second hop: account → contact
var contactLink = accountLink.AddLink("contact", "primarycontactid", "contactid");
contactLink.EntityAlias = "c";
contactLink.Columns = new ColumnSet("fullname");
```

### Link Criteria

Filter which linked rows participate in the join.

```csharp
var link = query.AddLink("contact", "primarycontactid", "contactid");
link.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, 0); // only active contacts
```

### Semi-Joins: Exists / In / Any

Return parent rows that have at least one matching child, without projecting child columns.

```csharp
// "Give me accounts that have at least one active contact"
var link = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Any);
link.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, 0);
```

`JoinOperator.Exists` and `JoinOperator.In` behave the same as `Any` — all three are semi-joins.

### Anti-Join: NotAny

Return parent rows with **no** matching child.

```csharp
// Accounts with no contacts
var link = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.NotAny);
```

### NotAll

Return parent rows where at least one key-matched child **fails** the link criteria.

```csharp
// Accounts where at least one contact is inactive
var link = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.NotAll);
link.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, 0);
```

---

## Paging

Use `PageInfo` to retrieve results in pages.

```csharp
// Seed 50 accounts
for (int i = 0; i < 50; i++)
    service.Create(new Entity("account") { ["name"] = $"Account {i:D2}" });

var query = new QueryExpression("account")
{
    ColumnSet = new ColumnSet("name")
};
query.PageInfo = new PagingInfo { PageNumber = 1, Count = 10 };

var allRecords = new List<Entity>();
while (true)
{
    var page = service.RetrieveMultiple(query);
    allRecords.AddRange(page.Entities);

    if (!page.MoreRecords)
        break;

    query.PageInfo.PageNumber++;
    query.PageInfo.PagingCookie = page.PagingCookie;
}

// allRecords.Count → 50
```

---

## TopCount

Limit results without paging.

```csharp
var query = new QueryExpression("account")
{
    ColumnSet = new ColumnSet("name"),
    TopCount = 5
};
query.AddOrder("name", OrderType.Ascending);

var results = service.RetrieveMultiple(query);
// results.Entities.Count → 5 (first 5 alphabetically)
```

---

## Distinct

Eliminate duplicate rows from the result set.

```csharp
var query = new QueryExpression("account")
{
    ColumnSet = new ColumnSet("name"),
    Distinct = true
};
```

---

## QueryByAttribute

A simpler alternative to `QueryExpression` when you only need equality conditions.

```csharp
var qba = new QueryByAttribute("account")
{
    ColumnSet = new ColumnSet("name", "revenue")
};
qba.AddAttributeValue("statecode", 0);
qba.AddAttributeValue("name", "Contoso");

var results = service.RetrieveMultiple(qba);
```

`QueryByAttribute` also supports `TopCount` and `PageInfo`.

---

## FetchXml

Direct FetchXml execution supports the same filter operator families documented
above plus `link-type` values `inner`, `outer`, `exists`, `in`, `any`,
`not-any`, `not-all`, and `natural`.

### Basic Query

```csharp
var fetchXml = @"
<fetch top='5'>
  <entity name='account'>
    <attribute name='name' />
    <attribute name='revenue' />
    <filter>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
    <order attribute='name' />
  </entity>
</fetch>";

var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
```

### Link-Entity in FetchXml

```csharp
var fetchXml = @"
<fetch>
  <entity name='account'>
    <attribute name='name' />
    <link-entity name='contact' from='contactid' to='primarycontactid' alias='c'>
      <attribute name='fullname' />
    </link-entity>
  </entity>
</fetch>";

var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
```

---

## FetchXml Aggregation

Set `aggregate='true'` on the `<fetch>` element to use aggregate functions: `count`, `countcolumn`, `sum`, `avg`, `min`, `max`.

### Simple Aggregate

```csharp
var fetchXml = @"
<fetch aggregate='true'>
  <entity name='account'>
    <attribute name='accountid' alias='total_count' aggregate='count' />
    <attribute name='revenue' alias='total_revenue' aggregate='sum' />
    <attribute name='revenue' alias='avg_revenue' aggregate='avg' />
  </entity>
</fetch>";

var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

var row = results.Entities[0];
var count = row.GetAttributeValue<AliasedValue>("total_count")?.Value;       // e.g., 10
var sum = row.GetAttributeValue<AliasedValue>("total_revenue")?.Value;       // e.g., 5000000m
var avg = row.GetAttributeValue<AliasedValue>("avg_revenue")?.Value;         // e.g., 500000m
```

### GroupBy

```csharp
var fetchXml = @"
<fetch aggregate='true'>
  <entity name='opportunity'>
    <attribute name='statecode' alias='status' groupby='true' />
    <attribute name='opportunityid' alias='count' aggregate='count' />
    <attribute name='estimatedvalue' alias='total_value' aggregate='sum' />
  </entity>
</fetch>";

var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
// One row per distinct statecode value
```

---

## FetchXml to QueryExpression

Convert FetchXml to a `QueryExpression` using the built-in request handler.
Aggregate FetchXml cannot be converted to `QueryExpression`.

```csharp
var response = (FetchXmlToQueryExpressionResponse)service.Execute(
    new FetchXmlToQueryExpressionRequest
    {
        FetchXml = @"
<fetch top='10'>
  <entity name='account'>
    <attribute name='name' />
    <filter>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>"
    });

var queryExpression = (QueryExpression)response["Query"];
```

## QueryExpression to FetchXml

Convert a supported `QueryExpression` back to FetchXml using the built-in
request handler.

```csharp
var query = new QueryExpression("account")
{
    ColumnSet = new ColumnSet("name"),
    Distinct = true,
    TopCount = 10
};
query.Criteria.AddCondition("name", ConditionOperator.BeginsWith, "Con");

var response = (QueryExpressionToFetchXmlResponse)service.Execute(
    new QueryExpressionToFetchXmlRequest
    {
        Query = query
    });

var fetchXml = (string)response.Results["FetchXml"];
```

The current serializer covers column projection / `all-attributes`, ordering,
`TopCount`, `Distinct`, nested filters, and nested link-entities for the join
and condition operators implemented by the handler. Unsupported operators or
join types throw `NotSupportedException`, and paging / `NoLock` are not emitted.

---

## Formatted Values

Every `Retrieve` and `RetrieveMultiple` response automatically populates `FormattedValues` for
well-known column types. Pre-existing formatted values on an entity are never overwritten —
your own values take priority.

| Attribute type | Format applied |
|---|---|
| `OptionSetValue` | Integer value as string, e.g. `"0"` |
| `Money` | `"#,##0.00"` format, e.g. `"1,500,000.00"` |
| `bool` | `"Yes"` or `"No"` |
| `DateTime` | `"M/d/yyyy h:mm tt"` format, e.g. `"4/12/2026 3:30 PM"` |

```csharp
var id = service.Create(new Entity("account")
{
    ["name"]       = "Contoso",
    ["revenue"]    = new Money(1500000m),
    ["statecode"]  = new OptionSetValue(0),
    ["createdon"]  = new DateTime(2026, 4, 12, 15, 30, 0, DateTimeKind.Utc)
});

var account = service.Retrieve("account", id, new ColumnSet(true));

Assert.Equal("1,500,000.00", account.FormattedValues["revenue"]);
Assert.Equal("0",            account.FormattedValues["statecode"]);
Assert.Equal("4/12/2026 3:30 PM", account.FormattedValues["createdon"]);
```

### Formatted Values on Aliased Columns

Aliased columns returned from `LinkEntity` joins and FetchXml aggregates also receive
auto-formatted values. The formatted value key matches the alias:

```csharp
var fetchXml = @"
<fetch>
  <entity name='account'>
    <attribute name='name' />
    <link-entity name='contact' from='accountid' to='accountid' alias='c'>
      <attribute name='revenue' />
    </link-entity>
  </entity>
</fetch>";

var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
var row = results.Entities[0];

// Aliased column formatted value is keyed by the alias
string? formattedRevenue = row.FormattedValues["c.revenue"]; // e.g., "50,000.00"
```

---

## Saved Queries

`service.ExecuteSavedQuery(Guid queryId)` executes a `userquery` or `savedquery` record that
has been seeded into the fake. It reads the record's `fetchxml` attribute and evaluates it via
`RetrieveMultiple(new FetchExpression(...))`.

```csharp
// Seed a saved query
var queryId = service.Create(new Entity("savedquery")
{
    ["name"]     = "Active Accounts",
    ["fetchxml"] = @"
<fetch>
  <entity name='account'>
    <attribute name='name' />
    <filter>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>"
});

// Seed some data
service.Create(new Entity("account") { ["name"] = "Contoso", ["statecode"] = new OptionSetValue(0) });
service.Create(new Entity("account") { ["name"] = "Inactive Co", ["statecode"] = new OptionSetValue(1) });

// Execute via saved query ID
EntityCollection results = service.ExecuteSavedQuery(queryId);

Assert.Single(results.Entities);
Assert.Equal("Contoso", results.Entities[0]["name"]);
```

The method looks for the record first in `userquery` and then in `savedquery` entity stores.

---

## Performance Tips

1. **Use `AttributeIndex`** for frequently filtered columns. Indexes turn full table scans into hash lookups.

   ```csharp
   env.AddIndex("account", "statecode");

   // Queries filtering on account.statecode with ConditionOperator.Equal now use the index
   ```

2. **Specify columns** — `new ColumnSet("name", "revenue")` instead of `new ColumnSet(true)`. This reduces per-entity cloning overhead.

3. **Use `TopCount`** when you only need the first N matches — avoids sorting and projecting the entire result set.
