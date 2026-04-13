# Cookbook — Common Testing Patterns

A collection of self-contained recipes for Fake4Dataverse. Each recipe is a complete
`[Fact]` test you can copy into your project.

> **Detailed guides**: [Pipeline & Plugin Testing](pipeline-plugin-testing.md) ·
> [Security & Access Control](security-access-control.md) ·
> [Assertion Adapters](assertion-adapters.md) ·
> [Querying](querying.md) ·
> [Metadata Validation](metadata-validation.md)

---

## 1. Testing a Real IPlugin End-to-End

Register your actual `IPlugin` class. Fake4Dataverse builds the same `IServiceProvider`
Dataverse provides at runtime — `IPluginExecutionContext`, `IOrganizationServiceFactory`,
and `ITracingService` all resolve automatically.

```csharp
[Fact]
public void MyAccountPlugin_CreatesRelatedContact_OnCreate()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, "account",
        new MyAccountPlugin());

    var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });

    var contacts = service.RetrieveMultiple(new QueryExpression("contact")
    {
        ColumnSet = new ColumnSet(true)
    });
    Assert.Single(contacts.Entities);
    Assert.Equal("Contoso Primary Contact", contacts.Entities[0]["lastname"]);
}
```

## 2. Simulating Plugin Logic With a Lambda

Test business logic without a full plugin class — useful for rapid prototyping.

```csharp
[Fact]
public void Lambda_CreatesRelatedContact_OnAccountCreate()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();

    env.Pipeline.RegisterPostOperation("Create", "account", ctx =>
    {
        var target = (Entity)ctx.InputParameters["Target"];
        service.Create(new Entity("contact")
        {
            ["parentcustomerid"] = new EntityReference("account", target.Id),
            ["lastname"] = target["name"] + " Primary Contact"
        });
    });

    service.Create(new Entity("account") { ["name"] = "Contoso" });

    var contacts = service.RetrieveMultiple(new QueryExpression("contact")
    {
        ColumnSet = new ColumnSet(true)
    });
    Assert.Single(contacts.Entities);
    Assert.Equal("Contoso Primary Contact", contacts.Entities[0]["lastname"]);
}
```

## 3. Asserting Plugin Traces

Traces written via `ITracingService` are captured in `Pipeline.Traces`.

```csharp
[Fact]
public void Plugin_TracesAreCaptured()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, new MyLoggingPlugin());

    service.Create(new Entity("account") { ["name"] = "Contoso" });

    Assert.Contains(env.Pipeline.Traces, t => t.Contains("Processing account"));
}
```

## 4. Cascade Delete

Define a one-to-many relationship with cascade delete, then verify children
are removed when the parent is deleted.

```csharp
[Fact]
public void DeleteAccount_CascadeDeletesContacts()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    env.MetadataStore.AddOneToManyRelationship(
        "account", "contact", "parentcustomerid",
        new Fake4Dataverse.Metadata.CascadeConfiguration
        {
            Delete = Fake4Dataverse.Metadata.CascadeType.Cascade
        });

    var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
    service.Create(new Entity("contact")
    {
        ["parentcustomerid"] = new EntityReference("account", accountId),
        ["lastname"] = "Smith"
    });

    service.Delete("account", accountId);

    var contacts = service.RetrieveMultiple(
        new QueryExpression("contact") { ColumnSet = new ColumnSet(true) });
    Assert.Empty(contacts.Entities);
}
```

## 5. Deterministic Time with FakeClock

Use `FakeClock` to control `UtcNow` and test time-dependent queries.

```csharp
[Fact]
public void Query_LastXDays_WithFakeClock()
{
    var clock = new FakeClock(new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc));
    var env = new FakeDataverseEnvironment() { Clock = clock };
    var service = env.CreateOrganizationService();

    clock.UtcNow = new DateTime(2024, 6, 10, 0, 0, 0, DateTimeKind.Utc);
    service.Create(new Entity("task") { ["subject"] = "Old task", ["createdon"] = clock.UtcNow });

    clock.UtcNow = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
    service.Create(new Entity("task") { ["subject"] = "New task", ["createdon"] = clock.UtcNow });

    var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
    query.Criteria.AddCondition("createdon", ConditionOperator.LastXDays, 3);
    var results = service.RetrieveMultiple(query);

    Assert.Single(results.Entities);
    Assert.Equal("New task", results.Entities[0]["subject"]);
}
```

## 6. ExecuteMultiple with Error Handling

Validates that `ContinueOnError` lets subsequent requests execute after a failure.

```csharp
[Fact]
public void ExecuteMultiple_ContinuesOnError()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

    var request = new ExecuteMultipleRequest
    {
        Requests = new OrganizationRequestCollection
        {
            new UpdateRequest { Target = new Entity("account", id) { ["name"] = "Updated" } },
            new DeleteRequest { Target = new EntityReference("account", Guid.NewGuid()) },
            new CreateRequest { Target = new Entity("account") { ["name"] = "New" } }
        },
        Settings = new ExecuteMultipleSettings
        {
            ContinueOnError = true,
            ReturnResponses = true
        }
    };

    var response = (ExecuteMultipleResponse)service.Execute(request);
    Assert.True(response.IsFaulted);  // second request failed
}
```

## 7. Test Isolation with Scopes

`Scope()` takes a snapshot and restores it on dispose — perfect for
shared-service test fixtures.

```csharp
[Fact]
public void ScopedTest_AutoRollback()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    service.Create(new Entity("account") { ["name"] = "Permanent" });

    using (env.Scope())
    {
        service.Create(new Entity("account") { ["name"] = "Temporary" });
        var all = service.RetrieveMultiple(
            new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
        Assert.Equal(2, all.Entities.Count);
    }

    var remaining = service.RetrieveMultiple(
        new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
    Assert.Single(remaining.Entities);
}
```

## 8. Verifying Operations with the Operation Log

The built-in operation log records every call for post-hoc assertions.

```csharp
[Fact]
public void OperationLog_RecordsAllCalls()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
    service.Update(new Entity("account", id) { ["name"] = "Updated" });
    service.Delete("account", id);

    Assert.True(service.OperationLog.HasCreated("account", id));
    Assert.True(service.OperationLog.HasUpdated("account", id));
    Assert.True(service.OperationLog.HasDeleted("account", id));

    var creates = service.OperationLog.GetOperations("Create");
    Assert.Single(creates);
}
```

## 9. Alternate Key Upsert

Register an alternate key, then upsert by key attributes — first call creates,
second call updates.

```csharp
[Fact]
public void Upsert_ByAlternateKey_CreatesOrUpdates()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    env.MetadataStore.AddEntity("account", "accountid", "name");
    env.MetadataStore.AddAlternateKey("account", "ak_account_number", "accountnumber");

    var target = new Entity("account") { ["accountnumber"] = "ACC-001", ["name"] = "Contoso" };
    target.KeyAttributes["accountnumber"] = "ACC-001";

    var response = (UpsertResponse)service.Execute(new UpsertRequest { Target = target });
    Assert.True(response.RecordCreated);

    target["name"] = "Contoso Updated";
    var response2 = (UpsertResponse)service.Execute(new UpsertRequest { Target = target });
    Assert.False(response2.RecordCreated);
}
```

## 10. Seeding Test Data from CSV

Load bulk test data in one call with inline CSV or from a file.

```csharp
[Fact]
public void SeedFromCsv_LoadsTestData()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    env.SeedFromCsv(@"logicalname,name,revenue
account,Contoso,1000000
account,Fabrikam,2000000
account,Northwind,500000");

    var results = service.RetrieveMultiple(
        new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
    Assert.Equal(3, results.Entities.Count);
}
```

File-based seeding is also available:

```csharp
env.SeedFromJsonFile("seed-data.json");
env.SeedFromCsvFile("seed-data.csv");
```

## 11. Associate / Disassociate (N:N Relationships)

Test many-to-many relationship operations. Associations are stored as queryable
`association_<relationshipname>` records.

```csharp
[Fact]
public void Associate_And_Disassociate_ManyToMany()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
    var contactId = service.Create(new Entity("contact") { ["lastname"] = "Doe" });

    service.Associate("account", accountId,
        new Relationship("account_contacts"),
        new EntityReferenceCollection { new EntityReference("contact", contactId) });

    var associations = service.RetrieveMultiple(
        new QueryExpression("association_account_contacts") { ColumnSet = new ColumnSet(true) });
    Assert.Single(associations.Entities);

    service.Disassociate("account", accountId,
        new Relationship("account_contacts"),
        new EntityReferenceCollection { new EntityReference("contact", contactId) });

    associations = service.RetrieveMultiple(
        new QueryExpression("association_account_contacts") { ColumnSet = new ColumnSet(true) });
    Assert.Empty(associations.Entities);
}
```

## 12. Testing WhoAmI

`WhoAmIRequest` is handled out of the box. Override the handler to control the
returned user/org/business-unit IDs.

```csharp
[Fact]
public void WhoAmI_ReturnsConfiguredUser()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    var customUserId = Guid.NewGuid();
    env.HandlerRegistry.Register(new Handlers.WhoAmIRequestHandler
    {
        UserId = customUserId
    });

    var response = (WhoAmIResponse)service.Execute(new WhoAmIRequest());

    Assert.Equal(customUserId, response.UserId);
    Assert.NotEqual(Guid.Empty, response.OrganizationId);
}
```

## 13. Testing SetState (Activate / Deactivate)

Use `SetStateRequest` to change `statecode` and `statuscode`, then verify.

```csharp
[Fact]
public void SetState_DeactivatesAndReactivates()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

    // Deactivate
    service.Execute(new SetStateRequest
    {
        EntityMoniker = new EntityReference("account", id),
        State = new OptionSetValue(1),   // Inactive
        Status = new OptionSetValue(2)
    });

    var deactivated = service.Retrieve("account", id, new ColumnSet("statecode", "statuscode"));
    Assert.Equal(1, deactivated.GetAttributeValue<OptionSetValue>("statecode").Value);
    Assert.Equal(2, deactivated.GetAttributeValue<OptionSetValue>("statuscode").Value);

    // Reactivate
    service.Execute(new SetStateRequest
    {
        EntityMoniker = new EntityReference("account", id),
        State = new OptionSetValue(0),   // Active
        Status = new OptionSetValue(1)
    });

    var reactivated = service.Retrieve("account", id, new ColumnSet("statecode"));
    Assert.Equal(0, reactivated.GetAttributeValue<OptionSetValue>("statecode").Value);
}
```

## 14. Testing Assign Request

`AssignRequest` changes the `ownerid` of a record.

```csharp
[Fact]
public void Assign_ChangesRecordOwner()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
    var newOwner = new EntityReference("systemuser", Guid.NewGuid());

    service.Execute(new AssignRequest
    {
        Target = new EntityReference("account", id),
        Assignee = newOwner
    });

    var retrieved = service.Retrieve("account", id, new ColumnSet("ownerid"));
    Assert.Equal(newOwner.Id, retrieved.GetAttributeValue<EntityReference>("ownerid").Id);
}
```

## 15. Custom Request Handlers

Register a handler for a custom API or override a built-in one. Use
`RegisterCustomApi` for simple cases, or implement `IOrganizationRequestHandler`
for full control.

```csharp
[Fact]
public void CustomApi_ReturnsExpectedOutput()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();

    env.RegisterCustomApi("myorg_ApproveOrder", (request, svc) =>
    {
        var orderId = (Guid)request["OrderId"];
        svc.Update(new Entity("salesorder", orderId) { ["statuscode"] = new OptionSetValue(100) });
        var response = new OrganizationResponse();
        response.Results["IsApproved"] = true;
        return response;
    });

    var orderId = service.Create(new Entity("salesorder") { ["name"] = "SO-001" });
    var req = new OrganizationRequest("myorg_ApproveOrder") { ["OrderId"] = orderId };
    var resp = service.Execute(req);

    Assert.True((bool)resp.Results["IsApproved"]);
    var order = service.Retrieve("salesorder", orderId, new ColumnSet("statuscode"));
    Assert.Equal(100, order.GetAttributeValue<OptionSetValue>("statuscode").Value);
}
```

## 16. Early-Bound Entities

Fake4Dataverse works seamlessly with early-bound (generated) entity classes.
Create, retrieve, and cast just like production code.

```csharp
// Simple early-bound class (typically generated by CrmSvcUtil / pac modelbuilder)
[Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("account")]
public sealed class Account : Entity
{
    public const string EntityLogicalName = "account";
    public Account() : base(EntityLogicalName) { }

    public string? Name
    {
        get => GetAttributeValue<string>("name");
        set => this["name"] = value;
    }
}

[Fact]
public void EarlyBound_CreateAndRetrieve()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    var id = service.Create(new Account { Name = "Contoso" });

    var retrieved = service.Retrieve("account", id, new ColumnSet(true));
    var typed = retrieved.ToEntity<Account>();

    Assert.Equal("Contoso", typed.Name);
}
```

## 17. Combining Pipeline and Security in One Test

Verifies that a plugin fires **and** security is enforced in the same scenario.

```csharp
[Fact]
public void Pipeline_And_Security_WorkTogether()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();
    env.Security.EnforceSecurityRoles = true;

    // Grant the caller Create + Read on account
    var role = new Security.SecurityRole("Sales")
        .AddPrivilege("account", Security.PrivilegeType.Create, Security.PrivilegeDepth.Organization)
        .AddPrivilege("account", Security.PrivilegeType.Read, Security.PrivilegeDepth.Organization);
    env.Security.AssignRole(service.CallerId, role);

    // Register a post-operation plugin that stamps a field
    env.Pipeline.RegisterPostOperation("Create", "account", ctx =>
    {
        var target = (Entity)ctx.InputParameters["Target"];
        service.Update(new Entity("account", target.Id) { ["description"] = "Processed" });
    });

    // Also grant Update so the plugin can write back
    role.AddPrivilege("account", Security.PrivilegeType.Write, Security.PrivilegeDepth.Organization);

    var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

    var account = service.Retrieve("account", id, new ColumnSet("description"));
    Assert.Equal("Processed", account["description"]);

    // Without Create privilege, a different user is blocked
    var otherUser = Guid.NewGuid();
    var otherService = env.CreateOrganizationService(otherUser);
    Assert.Throws<System.ServiceModel.FaultException<OrganizationServiceFault>>(() =>
        otherService.Create(new Entity("account") { ["name"] = "Blocked" }));
}
```

> For in-depth security configuration see the [Security & Access Control guide](security-access-control.md).
> For full pipeline registration options see the [Pipeline & Plugin Testing guide](pipeline-plugin-testing.md).

---

## 18. Status Transition Enforcement

Register valid transitions to make `SetStateRequest` enforce business rules:

```csharp
[Fact]
public void SetState_EnforcesRegisteredTransitions()
{
    var env = new FakeDataverseEnvironment();

    // Active(0/1) → Inactive(1/2)
    env.RegisterStatusTransition("lead", 0, 1, 1, 2);
    // Inactive(1/2) → Active(0/1)
    env.RegisterStatusTransition("lead", 1, 2, 0, 1);

    var service = env.CreateOrganizationService();
    var id = service.Create(new Entity("lead") { ["subject"] = "Hot Lead" });

    // Valid: deactivate
    service.Execute(new SetStateRequest
    {
        EntityMoniker = new EntityReference("lead", id),
        State  = new OptionSetValue(1),
        Status = new OptionSetValue(2)
    });

    // Invalid: unknown combination — throws
    Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
        service.Execute(new SetStateRequest
        {
            EntityMoniker = new EntityReference("lead", id),
            State  = new OptionSetValue(0),
            Status = new OptionSetValue(99)
        }));
}
```

---

## 19. Qualifying a Lead

`QualifyLeadRequest` converts a lead into contacts, accounts, and/or opportunities:

```csharp
[Fact]
public void QualifyLead_CreatesContactAndOpportunity()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();

    var leadId = service.Create(new Entity("lead")
    {
        ["subject"]   = "Hot Lead",
        ["firstname"] = "Alice",
        ["lastname"]  = "Smith"
    });

    var response = (QualifyLeadResponse)service.Execute(new QualifyLeadRequest
    {
        LeadId              = new EntityReference("lead", leadId),
        Status              = new OptionSetValue(3),    // Qualified
        CreateContact       = true,
        CreateOpportunity   = true,
        CreateAccount       = false
    });

    Assert.Contains(response.CreatedEntities, e => e.LogicalName == "contact");
    Assert.Contains(response.CreatedEntities, e => e.LogicalName == "opportunity");

    // Lead is disqualified
    var lead = service.Retrieve("lead", leadId, new ColumnSet("statecode"));
    Assert.Equal(1 /* Disqualified */, lead.GetAttributeValue<OptionSetValue>("statecode").Value);
}
```

---

## 20. Win / Lose an Opportunity

```csharp
[Fact]
public void WinOpportunity_SetsState()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();

    var oppId = service.Create(new Entity("opportunity") { ["name"] = "Deal" });

    // Create a close activity (required by WinOpportunityRequest)
    var closeId = service.Create(new Entity("opportunityclose")
    {
        ["opportunityid"] = new EntityReference("opportunity", oppId),
        ["subject"]       = "Close"
    });

    service.Execute(new WinOpportunityRequest
    {
        OpportunityClose = new Entity("opportunityclose", closeId),
        Status           = new OptionSetValue(3) // Won
    });

    var opp = service.Retrieve("opportunity", oppId, new ColumnSet("statecode", "statuscode"));
    Assert.Equal(1 /* Won */, opp.GetAttributeValue<OptionSetValue>("statecode").Value);
}
```

---

## 21. Queue Operations

Add and remove records from queues, and pick/release work items:

```csharp
[Fact]
public void Queue_AddPickAndRelease()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();

    var queueId = service.Create(new Entity("queue") { ["name"] = "Support" });
    var caseId  = service.Create(new Entity("incident") { ["title"] = "Issue #1" });

    // Add to queue — creates a queueitem record
    var addResponse = (AddToQueueResponse)service.Execute(new AddToQueueRequest
    {
        DestinationQueueId = queueId,
        Target             = new EntityReference("incident", caseId)
    });

    Guid queueItemId = addResponse.QueueItemId;

    // Pick the item (assign to user)
    service.Execute(new PickFromQueueRequest
    {
        QueueItemId           = queueItemId,
        WorkerId              = service.CallerId,
        RemoveQueueItem       = false
    });

    // Release back to queue
    service.Execute(new ReleaseToQueueRequest { QueueItemId = queueItemId });

    // Remove from queue
    service.Execute(new RemoveFromQueueRequest { QueueItemId = queueItemId });
}
```

---

## 22. Send an Email

`SendEmailRequest` marks an email entity as sent:

```csharp
[Fact]
public void SendEmail_SetsStateToClosed()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();

    var emailId = service.Create(new Entity("email")
    {
        ["subject"] = "Hello",
        ["statecode"]  = new OptionSetValue(0)
    });

    service.Execute(new SendEmailRequest
    {
        EmailId             = emailId,
        IssueSend           = true,
        TrackingToken       = string.Empty
    });

    var email = service.Retrieve("email", emailId, new ColumnSet("statecode", "statuscode"));
    // Sent: statecode=1, statuscode=3
    Assert.Equal(1, email.GetAttributeValue<OptionSetValue>("statecode").Value);
}
```

---

## 23. Email Merge from Template

`InstantiateTemplateRequest` produces an email entity pre-populated from a template, substituting
`{!entity:attribute;}` tokens with values from a target record:

```csharp
[Fact]
public void InstantiateTemplate_SubstitutesValues()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();

    var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
    var templateId = service.Create(new Entity("template")
    {
        ["title"]    = "Welcome",
        ["subject"]  = "Hello {!account:name;}",
        ["body"]     = "Dear {!account:name;}, welcome!"
    });

    var response = (InstantiateTemplateResponse)service.Execute(
        new InstantiateTemplateRequest
        {
            TemplateId     = templateId,
            ObjectType     = "account",
            ObjectId       = accountId
        });

    var email = response.EntityCollection.Entities[0];
    Assert.Equal("Hello Contoso", email["subject"]);
    Assert.Equal("Dear Contoso, welcome!", email["body"]);
}
```

---

## 24. Initialize a New Record from an Existing One

`InitializeFromRequest` copies nominated attributes from a source record to a new unsaved
entity, stripping system fields:

```csharp
[Fact]
public void InitializeFrom_CopiesSelectedAttributes()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();

    var sourceId = service.Create(new Entity("quote")
    {
        ["name"]       = "Q-001",
        ["description"] = "Original quote"
    });

    var response = (InitializeFromResponse)service.Execute(new InitializeFromRequest
    {
        EntityMoniker  = new EntityReference("quote", sourceId),
        TargetEntityName = "quote",
        TargetFieldType  = TargetFieldType.All
    });

    var newRecord = response.Entity;

    // System fields are stripped — record has a new ID but is not yet saved
    Assert.NotEqual(Guid.Empty, newRecord.Id);
    Assert.NotEqual(sourceId, newRecord.Id);
    Assert.False(newRecord.Contains("createdon"));

    // User-specified attributes are copied
    Assert.Equal("Q-001",             newRecord["name"]);
    Assert.Equal("Original quote",    newRecord["description"]);

    // Save the new record
    var newId = service.Create(newRecord);
    Assert.NotEqual(Guid.Empty, newId);
}
```

---

## 25. Auto-Number Fields

Seed an auto-number starting value and assert generated numbers increment correctly:

```csharp
[Fact]
public void AutoNumber_IncrementsFromSeed()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();

    // Set the seed for account.accountnumber
    service.Execute(new SetAutoNumberSeedRequest
    {
        EntityName     = "account",
        AttributeName  = "accountnumber",
        Value          = 1000
    });

    // Read back the current seed
    var seedResponse = (GetAutoNumberSeedResponse)service.Execute(
        new GetAutoNumberSeedRequest
        {
            EntityName     = "account",
            AttributeName  = "accountnumber"
        });
    Assert.Equal(1000L, seedResponse.AutoNumberSeedValue);

    // Get the next value — advances the counter
    var nextResponse = (GetNextAutoNumberValueResponse)service.Execute(
        new GetNextAutoNumberValueRequest
        {
            EntityName     = "account",
            AttributeName  = "accountnumber"
        });
    Assert.Equal("1000", nextResponse.AutoNumberValue); // or the format configured

    // Counter has incremented
    var nextResponse2 = (GetNextAutoNumberValueResponse)service.Execute(
        new GetNextAutoNumberValueRequest
        {
            EntityName    = "account",
            AttributeName = "accountnumber"
        });
    Assert.NotEqual(nextResponse.AutoNumberValue, nextResponse2.AutoNumberValue);
}
```

---

## 26. Merging Records

`MergeRequest` copies `UpdateContent` attributes onto the master record and deactivates the
subordinate:

```csharp
[Fact]
public void Merge_CopiesAttributesAndDeactivatesSubordinate()
{
    var env = new FakeDataverseEnvironment();
    var service = env.CreateOrganizationService();

    var masterId = service.Create(new Entity("account") { ["name"] = "Master Co" });
    var subId    = service.Create(new Entity("account") { ["name"] = "Sub Co", ["telephone1"] = "555-0100" });

    // Merge subId into masterId; supply attributes to copy onto master
    var updateContent = new Entity("account")
    {
        ["telephone1"] = "555-0200"
    };

    service.Execute(new MergeRequest
    {
        Target         = new EntityReference("account", masterId),
        SubordinateId  = subId,
        UpdateContent  = updateContent,
        PerformParentingChecks = false
    });

    // Master has the merged attribute
    var master = service.Retrieve("account", masterId, new ColumnSet("telephone1"));
    Assert.Equal("555-0200", master["telephone1"]);

    // Subordinate is deactivated
    var sub = service.Retrieve("account", subId, new ColumnSet("statecode"));
    Assert.Equal(1, sub.GetAttributeValue<OptionSetValue>("statecode").Value);
}
```

---

## 27. Solution-Aware Publish Workflow

Register entity types as solution-aware and test the draft → publish lifecycle:

```csharp
[Fact]
public void PublishXml_PromotesDraftToPublished()
{
    var env = new FakeDataverseEnvironment();
    env.RegisterSolutionAwareEntity("webresource");
    var service = env.CreateOrganizationService();

    // Create goes to unpublished store
    var id = service.Create(new Entity("webresource")
    {
        ["name"]    = "new_script.js",
        ["content"] = "v1"
    });

    // Normal Retrieve throws — record is not yet published
    Assert.Throws<FaultException<OrganizationServiceFault>>(
        () => service.Retrieve("webresource", id, new ColumnSet(true)));

    // Read draft via unpublished request
    var draft = ((RetrieveUnpublishedResponse)service.Execute(
        new RetrieveUnpublishedRequest
        {
            Target    = new EntityReference("webresource", id),
            ColumnSet = new ColumnSet("content", "componentstate")
        })).Entity;
    Assert.Equal("v1", draft["content"]);
    Assert.Equal(1, draft.GetAttributeValue<OptionSetValue>("componentstate").Value);

    // Publish — promotes to main store
    service.Execute(new PublishXmlRequest
    {
        ParameterXml = "<importexportxml><webresources><webresource>" +
                       $"{{{id}}}" +
                       "</webresource></webresources></importexportxml>"
    });

    // Normal Retrieve now works
    var published = service.Retrieve("webresource", id, new ColumnSet("content", "componentstate"));
    Assert.Equal("v1", published["content"]);
    Assert.Equal(0, published.GetAttributeValue<OptionSetValue>("componentstate").Value);
}
```

See the [Solution-Aware Entities guide](solution-aware-entities.md) for the full workflow.
