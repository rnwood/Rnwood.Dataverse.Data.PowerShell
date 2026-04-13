# Calculated & Rollup Fields

Fake4Dataverse includes a `CalculatedFieldManager` that evaluates calculated and rollup fields **on Retrieve**, not on write. This mirrors Dataverse behavior where calculated/rollup columns are computed when a record is read.

Access the manager via `env.CalculatedFields`.

---

## Registering Calculated Fields

Use `RegisterCalculatedField` to define a formula that computes a field value from the entity's attributes:

```csharp
env.CalculatedFields.RegisterCalculatedField(
    entityName: "contact",
    attributeName: "fullname",
    formula: e => $"{e.GetAttributeValue<string>("firstname")} {e.GetAttributeValue<string>("lastname")}");
```

The formula receives the full `Entity` and returns the computed value. It runs every time the entity is retrieved.

### Numeric Calculation Example

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();

env.CalculatedFields.RegisterCalculatedField(
    "orderdetail",
    "extendedamount",
    e =>
    {
        var qty = e.GetAttributeValue<decimal?>("quantity") ?? 0m;
        var price = e.GetAttributeValue<Money>("priceperunit")?.Value ?? 0m;
        return new Money(qty * price);
    });

var id = service.Create(new Entity("orderdetail")
{
    ["quantity"] = 5m,
    ["priceperunit"] = new Money(20m)
});

var record = service.Retrieve("orderdetail", id, new ColumnSet(true));
Assert.Equal(100m, record.GetAttributeValue<Money>("extendedamount").Value);
```

---

## Registering Rollup Fields

Rollup fields aggregate values from related entities. Use `RegisterRollupField`:

```csharp
env.CalculatedFields.RegisterRollupField(
    entityName: "account",
    attributeName: "totalrevenue",
    relatedEntity: "opportunity",
    relatedAttribute: "estimatedvalue",
    lookupAttribute: "parentaccountid",
    aggregateType: RollupType.Sum);
```

The `RollupType` enum supports: `Sum`, `Count`, `Avg`, `Min`, `Max`.

### Count with Filter Example

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();

var filter = new FilterExpression();
filter.AddCondition("statecode", ConditionOperator.Equal, 0); // active only

env.CalculatedFields.RegisterRollupField(
    entityName: "account",
    attributeName: "activecontactcount",
    relatedEntity: "contact",
    relatedAttribute: "contactid",   // ignored for Count
    lookupAttribute: "parentcustomerid",
    aggregateType: RollupType.Count,
    filter: filter);

var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
var accountRef = new EntityReference("account", accountId);

service.Create(new Entity("contact") { ["parentcustomerid"] = accountRef, ["statecode"] = 0 });
service.Create(new Entity("contact") { ["parentcustomerid"] = accountRef, ["statecode"] = 0 });
service.Create(new Entity("contact") { ["parentcustomerid"] = accountRef, ["statecode"] = 1 });

var account = service.Retrieve("account", accountId, new ColumnSet(true));
Assert.Equal(2, Convert.ToInt32(account["activecontactcount"]));
```

---

## CalculateRollupFieldRequest

You can also trigger rollup evaluation via `Execute`:

```csharp
var response = (CalculateRollupFieldResponse)service.Execute(
    new CalculateRollupFieldRequest
    {
        Target = new EntityReference("account", accountId),
        FieldName = "totalrevenue"
    });

var value = response.Entity.GetAttributeValue<Money>("totalrevenue");
```

This evaluates the registered rollup and returns the parent entity with the computed field.

---

## Tips

- **Register once** — define calculated and rollup fields in your test fixture setup or constructor, before creating data.
- **ColumnSet filtering** — calculated fields are applied after retrieval, so they work correctly even when using a specific `ColumnSet`.
- **No write-side effects** — formulas are never stored in the entity store; they are evaluated fresh on every `Retrieve` or `RetrieveMultiple`.
- **Rollup filter** — the optional `FilterExpression` parameter lets you restrict which related records are included in the aggregation.
- **Combine both** — you can register both calculated and rollup fields on the same entity. Rollup fields are evaluated first, so a calculated field can reference a rollup result.
