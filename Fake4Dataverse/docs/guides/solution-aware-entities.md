# Solution-Aware Entities (Publish / Unpublish)

Some Dataverse entity types — such as web resources, forms, and sitemap fragments — have a
publish lifecycle: changes are staged as **unpublished drafts** and only become visible to
end-users after an explicit publish step. Fake4Dataverse models this with a separate unpublished
store and four dedicated request handlers.

---

## Concepts

| Term | Meaning |
|---|---|
| **Solution-aware entity** | An entity type whose records go through a draft → published lifecycle, tracked via the `componentstate` attribute |
| **`componentstate = 0`** | Published — visible via normal `Retrieve`/`RetrieveMultiple` |
| **`componentstate = 1`** | Unpublished — only visible via `RetrieveUnpublishedRequest` / `RetrieveUnpublishedMultipleRequest` |
| **`PublishXmlRequest`** | Publishes specific records by entity type (and optionally by ID) |
| **`PublishAllXmlRequest`** | Publishes all pending drafts across all solution-aware entity types |

---

## Registering a Solution-Aware Entity

Call `RegisterSolutionAwareEntity` on the environment before any CRUD operations:

```csharp
var env = new FakeDataverseEnvironment();
env.RegisterSolutionAwareEntity("webresource");

var service = env.CreateOrganizationService();
```

After registration every `Create` and `Update` on `webresource` records is routed to the
unpublished store instead of the main store. Normal `Retrieve` and `RetrieveMultiple` see only
published records; the unpublished store is queried only via the dedicated request handlers.

You can check whether an entity type is registered:

```csharp
bool isSolutionAware = env.IsSolutionAwareEntity("webresource"); // true
```

An entity type is also treated as solution-aware if its registered metadata contains a
`componentstate` attribute — so metadata-driven detection works without an explicit call.

---

## Create → Unpublished

```csharp
var id = service.Create(new Entity("webresource")
{
    ["name"]    = "new_myscript.js",
    ["content"] = "alert('hello');"
});

// Record exists in the unpublished store — componentstate = 1 (Unpublished)
// Normal Retrieve returns EntityNotFoundException because it is not yet published.
```

---

## Retrieving Unpublished Records

Use `RetrieveUnpublishedRequest` and `RetrieveUnpublishedMultipleRequest` to read drafts:

```csharp
using Microsoft.Crm.Sdk.Messages;

// Single record
var retrieveResponse = (RetrieveUnpublishedResponse)service.Execute(
    new RetrieveUnpublishedRequest
    {
        Target    = new EntityReference("webresource", id),
        ColumnSet = new ColumnSet(true)
    });

Entity draft = retrieveResponse.Entity;
Assert.Equal(1, draft.GetAttributeValue<OptionSetValue>("componentstate").Value); // Unpublished
```

```csharp
// Collection
var multiResponse = (RetrieveUnpublishedMultipleResponse)service.Execute(
    new RetrieveUnpublishedMultipleRequest
    {
        Query = new QueryExpression("webresource") { ColumnSet = new ColumnSet(true) }
    });

EntityCollection drafts = multiResponse.EntityCollection;
```

---

## Publishing Records

### PublishXmlRequest — targeted publish

`PublishXmlRequest.ParameterXml` is an XML string describing what to publish. Fake4Dataverse
recognises `<webresource>` elements and publishes all unpublished records of that entity type:

```csharp
using Microsoft.Crm.Sdk.Messages;

service.Execute(new PublishXmlRequest
{
    ParameterXml = "<importexportxml><webresources>" +
                   $"<webresource>{{{id}}}</webresource>" +
                   "</webresources></importexportxml>"
});

// Now the record is in the main store with componentstate = 0 (Published)
var published = service.Retrieve("webresource", id, new ColumnSet("name", "componentstate"));
Assert.Equal(0, published.GetAttributeValue<OptionSetValue>("componentstate").Value);
```

The entity type tag in the XML is matched case-insensitively against registered solution-aware
entity names. All pending drafts for matching entity types are published.

### PublishAllXmlRequest — publish everything

```csharp
service.Execute(new PublishAllXmlRequest());

// All pending drafts across all registered solution-aware entity types are now published.
```

---

## Update → Unpublished Merge

When you `Update` an already-published record, the fake copies the published record into the
unpublished store (if not already there), applies your changes, and marks it unpublished until
the next publish:

```csharp
// Suppose "webresource" id is already published.
service.Update(new Entity("webresource", id)
{
    ["content"] = "alert('updated');"
});

// Published version is unchanged — update was staged.
var still_old = service.Retrieve("webresource", id, new ColumnSet("content"));
Assert.Equal("alert('hello');", still_old["content"]);

// Unpublished version has the new content.
var draft = ((RetrieveUnpublishedResponse)service.Execute(
    new RetrieveUnpublishedRequest { Target = new EntityReference("webresource", id), ColumnSet = new ColumnSet("content") })).Entity;
Assert.Equal("alert('updated');", draft["content"]);

// Publish to promote.
service.Execute(new PublishAllXmlRequest());
var updated = service.Retrieve("webresource", id, new ColumnSet("content"));
Assert.Equal("alert('updated');", updated["content"]);
```

---

## Delete

`Delete` removes the record from **both** the published and unpublished stores simultaneously:

```csharp
service.Delete("webresource", id);
// Record no longer exists in either store.
```

---

## Snapshot / Scope Compatibility

Snapshots captured via `env.TakeSnapshot()` / `env.Scope()` include both the published and
unpublished stores, as well as the solution-aware entity registrations. Rolling back a snapshot
restores the complete publish state.

---

## Tips

- Register solution-aware entities **before** seeding or running CRUD. Entities seeded via `env.Seed()` bypass the routing logic and land directly in the published store with `componentstate = 0`.
- `Retrieve` on an unpublished-only record throws the same `FaultException<OrganizationServiceFault>` (error code `0x80040217`) as a missing record — matching real Dataverse behaviour.
- Use `env.IsSolutionAwareEntity(name)` in custom handlers or pipeline steps to guard logic that should run only for solution-aware entity types.
