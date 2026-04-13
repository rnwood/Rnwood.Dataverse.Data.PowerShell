# Metadata & Validation

Fake4Dataverse can optionally validate Create and Update operations against registered entity/attribute metadata, catching schema errors in tests the same way Dataverse would at runtime.

## Overview

- Metadata validation is **off by default** — entities and attributes work without any metadata registered.
- Enable validation with the `ValidateWithMetadata` option or the `Strict` preset.
- All metadata lives in `env.MetadataStore`, an `InMemoryMetadataStore`.
- **Auto-discovery mode** (`AutoDiscoverMetadata = true`) infers attribute types from entity data on the first Create.
- **XML loading** — export real metadata from a live environment with the CLI tool and load the files in your tests.

## Loading Real Metadata from XML Files

The fastest way to get accurate table metadata is to export it from a real Dataverse environment using the `Fake4Dataverse.Tool` CLI tool and then load the resulting files in your test setup.

### Step 1 — Export metadata

Install the tool (once):

```
dotnet tool install --global Fake4Dataverse.Tool
```

Export one or more tables by name:

```
fake4dataverse export-metadata \
  --url https://org.crm.dynamics.com/ \
  --tables account contact \
  --output ./TestMetadata
```

Export all tables from a solution:

```
fake4dataverse export-metadata \
  --url https://org.crm.dynamics.com/ \
  --solutions MySolution \
  --output ./TestMetadata
```

Authentication uses interactive browser sign-in. A token cache is stored in
`%LOCALAPPDATA%/Fake4Dataverse/` so subsequent runs do not prompt again.

Each exported file is a DataContract-serialized `EntityMetadata` XML named
`<logicalname>.xml` (e.g. `account.xml`, `contact.xml`).

### Step 2 — Load in tests

```csharp
var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions
{
    ValidateWithMetadata = true,
});

// Load a single table
env.LoadMetadataFromXmlFile("TestMetadata/account.xml");

// Or load all files in a folder
foreach (var file in Directory.GetFiles("TestMetadata", "*.xml"))
    env.LoadMetadataFromXmlFile(file);

var service = env.CreateOrganizationService();
```

You can also load from an XML string (e.g. embedded resource):

```csharp
var xml = File.ReadAllText("TestMetadata/contact.xml");
env.LoadMetadataFromXml(xml);
```

### Solution-aware tables

If a table's exported metadata includes a `componentstate` attribute (standard for solution-aware tables such as web resources, saved queries, etc.) it is **automatically registered as solution-aware** — no extra call to `RegisterSolutionAwareEntity` is needed. Records created or updated for such tables are staged as unpublished until published via `PublishXmlRequest` or `PublishAllXmlRequest`.

## Enabling Validation

```csharp
// Option 1: Use the Strict preset (also enables security, pipeline, etc.)
var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Strict);
var service = env.CreateOrganizationService();

// Option 2: Enable validation on an existing environment
var env = new FakeDataverseEnvironment();
env.Options.ValidateWithMetadata = true;
var service = env.CreateOrganizationService();
```

With validation off (the default), any attribute name and value is accepted:

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
env.MetadataStore.AddEntity("account")
    .WithStringAttribute("name", maxLength: 5);

// Succeeds despite violating maxLength — validation is off.
service.Create(new Entity("account") { ["name"] = "VeryLongName" });
```

## Registering Entity Metadata

Use the fluent `EntityMetadataBuilder` returned by `AddEntity`:

```csharp
env.MetadataStore.AddEntity("account")
    .WithPrimaryIdAttribute("accountid")
    .WithPrimaryNameAttribute("name")
    .WithSchemaName("Account")
    .WithObjectTypeCode(1);
```

Calling `AddEntity` with an existing logical name returns the existing builder, so you can add attributes incrementally.

## Attribute Types

The builder provides typed methods for every common Dataverse attribute type:

```csharp
env.MetadataStore.AddEntity("account")
    // Strings — optional maxLength
    .WithStringAttribute("name", maxLength: 200)
    .WithStringAttribute("description")

    // Numeric — optional min/max
    .WithIntegerAttribute("numberofemployees", minValue: 0, maxValue: 1000000)
    .WithDecimalAttribute("exchangerate", minValue: 0.0001m, maxValue: 100m)
    .WithDoubleAttribute("latitude", minValue: -90, maxValue: 90)
    .WithMoneyAttribute("revenue", minValue: 0, maxValue: 100000000)

    // Boolean
    .WithBooleanAttribute("donotphone")

    // DateTime
    .WithDateTimeAttribute("lastonholdtime")

    // Picklist — optional set of valid values
    .WithOptionSetAttribute("industrycode", validValues: new[] { 1, 2, 3 })

    // Lookup — optional target entity types
    .WithLookupAttribute("primarycontactid", targetEntityTypes: new[] { "contact" })

    // Generic fallback
    .WithAttribute("statuscode", AttributeTypeCode.Status);
```

Every method accepts an optional `requiredLevel` parameter (see below).

## Required Level

The `AttributeRequiredLevel` enum controls whether a field must be present:

| Value | Behavior |
|---|---|
| `None` (default) | No requirement — field can be omitted or null. |
| `SystemRequired` | Must be present and non-null on Create. Cannot be set to null on Update. |
| `ApplicationRequired` | Same enforcement as `SystemRequired`. |
| `Recommended` | No enforcement (advisory only). |

```csharp
var env = new FakeDataverseEnvironment();
env.Options.ValidateWithMetadata = true;
var service = env.CreateOrganizationService();
env.MetadataStore.AddEntity("contact")
    .WithStringAttribute("lastname", requiredLevel: AttributeRequiredLevel.SystemRequired);

// Throws — lastname is missing
service.Create(new Entity("contact") { ["firstname"] = "John" });

// Succeeds
service.Create(new Entity("contact") { ["lastname"] = "Doe" });

// Throws — cannot null out a required field
var id = service.Create(new Entity("contact") { ["lastname"] = "Doe" });
service.Update(new Entity("contact", id) { ["lastname"] = null });
```

## Validation Behavior

When `ValidateWithMetadata = true` and metadata is registered for the entity:

**On Create:**
- Required fields (`SystemRequired` / `ApplicationRequired`) must be present and non-null.
- String values are checked against `maxLength`.
- Numeric values (int, decimal, double, Money) are checked against `minValue` / `maxValue`.
- `OptionSetValue` is checked against `validValues` (if defined).
- `EntityReference` lookup target type is checked against `targetEntityTypes` (if defined).

**On Update:**
- Required fields cannot be set to `null`.
- All type/range constraints apply to the provided attributes only.

All validation failures throw `FaultException<OrganizationServiceFault>` with a descriptive message:

```csharp
var env = new FakeDataverseEnvironment();
env.Options.ValidateWithMetadata = true;
var service = env.CreateOrganizationService();
env.MetadataStore.AddEntity("account")
    .WithIntegerAttribute("numberofemployees", minValue: 0, maxValue: 1000)
    .WithOptionSetAttribute("industrycode", validValues: new[] { 1, 2, 3 })
    .WithLookupAttribute("primarycontactid", targetEntityTypes: new[] { "contact" });

// Integer above maximum
service.Create(new Entity("account") { ["numberofemployees"] = 2000 });
// => "Value 2000 is above maximum 1000 ..."

// Invalid option set value
service.Create(new Entity("account") { ["industrycode"] = new OptionSetValue(99) });
// => "... invalid option set value 99 ..."

// Wrong lookup target
service.Create(new Entity("account")
{
    ["primarycontactid"] = new EntityReference("lead", Guid.NewGuid())
});
// => "... does not accept entity type 'lead' ..."
```

## Relationships

### 1:N Relationships

```csharp
env.MetadataStore.AddOneToManyRelationship(
    schemaName: "account_contacts",
    referencedEntity: "account",
    referencedAttribute: "accountid",
    referencingEntity: "contact",
    referencingAttribute: "parentcustomerid");
```

Or use the fluent builder:

```csharp
env.MetadataStore.AddEntity("account")
    .WithOneToManyRelationship(
        "account_contacts",
        "account", "accountid",
        "contact", "parentcustomerid");
```

### N:N Relationships

```csharp
env.MetadataStore.AddManyToManyRelationship(
    schemaName: "account_contact_nn",
    entity1LogicalName: "account",
    entity2LogicalName: "contact",
    intersectEntityName: "accountcontact_association");
```

Or via the fluent builder:

```csharp
env.MetadataStore.AddEntity("account")
    .WithManyToManyRelationship("account_contact_nn", "account", "contact");
```

## Cascade Configuration

Attach a `CascadeConfiguration` to a 1:N relationship to control what happens to child records when the parent is deleted, assigned, shared, etc.

```csharp
using Fake4Dataverse.Metadata;

env.MetadataStore.AddOneToManyRelationship(
    "account_contacts",
    "account", "accountid",
    "contact", "parentcustomerid",
    new CascadeConfiguration
    {
        Delete = CascadeType.Cascade,    // Delete children when parent is deleted
        Assign = CascadeType.Cascade,    // Re-assign children when parent owner changes
        Reparent = CascadeType.NoCascade,
        Share = CascadeType.NoCascade,
        Unshare = CascadeType.NoCascade
    });
```

`CascadeType` values:

| Value | Effect |
|---|---|
| `NoCascade` | No effect on child records. |
| `Cascade` | Apply the same operation to all child records. |
| `Active` | Apply only to active child records. |
| `UserOwned` | Apply only to children owned by the same user. |
| `RemoveLink` | Clear the foreign key (set lookup to null). |
| `Restrict` | Block the parent operation if children exist. |

```csharp
// With Restrict — deleting a parent with children throws
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
env.MetadataStore.AddOneToManyRelationship(
    "account_contacts", "account", "accountid", "contact", "parentcustomerid",
    new CascadeConfiguration { Delete = CascadeType.Restrict });

var parentId = service.Create(new Entity("account") { ["name"] = "Parent" });
service.Create(new Entity("contact") { ["parentcustomerid"] = parentId.ToEntityReference("account") });

// Throws — cannot delete parent while children reference it
service.Delete("account", parentId);
```

## Alternate Keys

Define alternate keys to enable Retrieve and Upsert by `KeyAttributeCollection`:

```csharp
env.MetadataStore.AddEntity("account")
    .WithAlternateKey("ak_accountnumber", "accountnumber");

// Composite keys
env.MetadataStore.AddEntity("contact")
    .WithAlternateKey("ak_name", "firstname", "lastname");
```

Once defined, you can use `KeyAttributeCollection` in entity references:

```csharp
var entity = new Entity("account")
{
    KeyAttributes = { { "accountnumber", "ACC-001" } },
    ["name"] = "Updated Name"
};
service.Upsert(new UpsertRequest { Target = entity });
```

## Early-Bound Entity Registration

Instead of manually registering every attribute, scan your early-bound classes:

```csharp
using Fake4Dataverse.EarlyBound;

// Register all entities from an assembly
env.RegisterEarlyBoundEntities(typeof(Account).Assembly);

// Or register a single entity type
env.RegisterEarlyBoundEntity<Account>();
```

This scans `[EntityLogicalName]` and `[AttributeLogicalName]` attributes on the class and its properties, and automatically:
- Creates entity metadata with the correct logical name.
- Sets `PrimaryIdAttribute` (convention: `{entityname}id`).
- Detects `PrimaryNameAttribute` from common name patterns.
- Maps .NET property types to `AttributeTypeCode` (string → `String`, int → `Integer`, `Money` → `Money`, `EntityReference` → `Lookup`, `OptionSetValue` → `Picklist`, etc.).

## Metadata Execute Requests

The fake service handles standard metadata SDK requests so you can query metadata in your code under test:

| Request | Description |
|---|---|
| `RetrieveEntityRequest` | Returns `EntityMetadata` for a single entity. |
| `RetrieveAllEntitiesRequest` | Returns metadata for all registered entities. |
| `RetrieveAttributeRequest` | Returns `AttributeMetadata` for a single attribute. |
| `CreateEntityRequest` | Programmatically register entity metadata. |
| `UpdateEntityRequest` / `DeleteEntityRequest` | Modify or remove entity metadata. |
| `CreateAttributeRequest` / `UpdateAttributeRequest` / `DeleteAttributeRequest` | Manage attribute metadata. |
| `CreateOneToManyRequest` / `CreateManyToManyRequest` / `DeleteRelationshipRequest` | Manage relationships. |
| `CreateEntityKeyRequest` / `DeleteEntityKeyRequest` / `RetrieveEntityKeyRequest` | Manage alternate keys. |
| `CreateOptionSetRequest` / `UpdateOptionSetRequest` / `DeleteOptionSetRequest` | Manage global option sets. |
| `InsertOptionValueRequest` / `UpdateOptionValueRequest` / `DeleteOptionValueRequest` | Manage option set values. |

```csharp
// Query metadata the same way production code would
var response = (RetrieveEntityResponse)service.Execute(
    new RetrieveEntityRequest { LogicalName = "account" });

Assert.Equal("Account", response.EntityMetadata.SchemaName);
Assert.Contains(response.EntityMetadata.Attributes,
    a => a.LogicalName == "name");
```

## Auto-Discovery Mode

When `AutoDiscoverMetadata` is enabled, the store infers attribute types from the values in the first Create call:

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
env.MetadataStore.AutoDiscoverMetadata = true;

service.Create(new Entity("account")
{
    ["name"] = "Contoso",                                        // inferred as String
    ["numberofemployees"] = 500,                                 // inferred as Integer
    ["revenue"] = new Money(1000m),                              // inferred as Money
    ["primarycontactid"] = new EntityReference("contact", Guid.NewGuid())  // inferred as Lookup
});

// Metadata is now queryable
var response = (RetrieveEntityResponse)service.Execute(
    new RetrieveEntityRequest { LogicalName = "account" });
Assert.Contains(response.EntityMetadata.Attributes, a => a.LogicalName == "name");
```

Auto-discovery does **not** overwrite explicitly registered metadata — you can combine both approaches:

```csharp
env.MetadataStore.AutoDiscoverMetadata = true;
env.MetadataStore.AddEntity("account")
    .WithStringAttribute("name", maxLength: 100);

// "name" retains its explicit maxLength; "phone" is auto-discovered
service.Create(new Entity("account") { ["name"] = "Contoso", ["phone"] = "555-1234" });
```

## Tips

- **Register metadata once** in a shared test fixture or constructor for performance — don't repeat it in every test.
- **Use `Strict` preset** when you want production-realistic validation without configuring each option.
- **Early-bound registration is the easiest path** to full metadata — one line replaces dozens of `WithAttribute` calls.
- **Auto-discovery** is a good middle ground if you don't need constraint validation but want metadata available for `RetrieveEntityRequest`.
- **Combine approaches**: register key constraints explicitly (required fields, max lengths, valid option sets), then let auto-discovery handle the rest.
