using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    // Simple early-bound entity for testing (simulates generated code)
    [Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("account")]
    public sealed class AccountEntity : Entity
    {
        public const string EntityLogicalName = "account";
        public AccountEntity() : base(EntityLogicalName) { }

        public string? Name
        {
            get { return GetAttributeValue<string>("name"); }
            set { this["name"] = value; }
        }

        public Money? Revenue
        {
            get { return GetAttributeValue<Money>("revenue"); }
            set { this["revenue"] = value; }
        }

        public EntityReference? PrimaryContactId
        {
            get { return GetAttributeValue<EntityReference>("primarycontactid"); }
            set { this["primarycontactid"] = value; }
        }
    }

    [Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("contact")]
    public sealed class ContactEntity : Entity
    {
        public const string EntityLogicalName = "contact";
        public ContactEntity() : base(EntityLogicalName) { }

        public string? FullName
        {
            get { return GetAttributeValue<string>("fullname"); }
            set { this["fullname"] = value; }
        }

        public EntityReference? ParentCustomerId
        {
            get { return GetAttributeValue<EntityReference>("parentcustomerid"); }
            set { this["parentcustomerid"] = value; }
        }
    }

    public class QueryPowerTests
    {
        // ===== 6.1 ColumnSet Enhancements =====

        [Fact]
        public void ColumnSet_AddColumn_ProjectsAddedColumns()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso", ["revenue"] = new Money(5000m), ["city"] = "Seattle" });

            var query = new QueryExpression("account");
            query.ColumnSet = new ColumnSet();
            query.ColumnSet.AddColumn("name");
            query.ColumnSet.AddColumn("revenue");
            // "city" deliberately not added

            var result = service.RetrieveMultiple(query);

            Assert.Single(result.Entities);
            Assert.Equal("Contoso", result.Entities[0].GetAttributeValue<string>("name"));
            Assert.Equal(5000m, result.Entities[0].GetAttributeValue<Money>("revenue").Value);
            Assert.False(result.Entities[0].Contains("city"), "city should not be projected");
        }

        [Fact]
        public void ColumnSet_AddColumns_ProjectsAllAdded()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso", ["revenue"] = new Money(100m), ["city"] = "Redmond" });

            var query = new QueryExpression("account");
            query.ColumnSet = new ColumnSet();
            query.ColumnSet.AddColumns("name", "city");

            var result = service.RetrieveMultiple(query);

            Assert.Single(result.Entities);
            Assert.True(result.Entities[0].Contains("name"));
            Assert.True(result.Entities[0].Contains("city"));
            Assert.False(result.Entities[0].Contains("revenue"), "revenue should not be projected");
        }

        [Fact]
        public void LinkEntity_AddColumn_ProjectsAliasedColumns()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var acctId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("contact") { ["fullname"] = "Alice", ["email"] = "alice@contoso.com", ["parentcustomerid"] = new EntityReference("account", acctId) });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var link = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Inner);
            link.EntityAlias = "c";
            link.Columns = new ColumnSet();
            link.Columns.AddColumn("fullname");
            // "email" deliberately not added via AddColumn

            var result = service.RetrieveMultiple(query);

            Assert.Single(result.Entities);
            Assert.True(result.Entities[0].Contains("c.fullname"));
            Assert.False(result.Entities[0].Contains("c.email"), "email should not be projected on link");
        }

        [Fact]
        public void LinkEntity_AddColumns_ProjectsMultipleAliasedColumns()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var acctId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("contact") { ["fullname"] = "Alice", ["email"] = "alice@contoso.com", ["parentcustomerid"] = new EntityReference("account", acctId) });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var link = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Inner);
            link.EntityAlias = "c";
            link.Columns = new ColumnSet();
            link.Columns.AddColumns("fullname", "email");

            var result = service.RetrieveMultiple(query);

            Assert.Single(result.Entities);
            Assert.True(result.Entities[0].Contains("c.fullname"));
            Assert.True(result.Entities[0].Contains("c.email"));
            var aliasedEmail = result.Entities[0].GetAttributeValue<AliasedValue>("c.email");
            Assert.Equal("alice@contoso.com", aliasedEmail.Value);
        }

        // ===== 6.2 Early-Bound Parity =====

        [Fact]
        public void EarlyBound_Create_Works()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var account = new AccountEntity { Name = "Contoso", Revenue = new Money(10000m) };

            var id = service.Create(account);

            Assert.NotEqual(Guid.Empty, id);
        }

        [Fact]
        public void EarlyBound_Retrieve_CanCastToEarlyBound()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var account = new AccountEntity { Name = "Contoso", Revenue = new Money(10000m) };
            var id = service.Create(account);

            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            var typed = retrieved.ToEntity<AccountEntity>();

            Assert.Equal("Contoso", typed.Name);
            Assert.NotNull(typed.Revenue);
            Assert.Equal(10000m, typed.Revenue!.Value);
        }

        [Fact]
        public void EarlyBound_Update_Works()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var account = new AccountEntity { Name = "Contoso" };
            var id = service.Create(account);

            var update = new AccountEntity { Id = id, Name = "Fabrikam" };
            service.Update(update);

            var retrieved = service.Retrieve("account", id, new ColumnSet("name"));
            var typed = retrieved.ToEntity<AccountEntity>();
            Assert.Equal("Fabrikam", typed.Name);
        }

        [Fact]
        public void EarlyBound_Delete_Works()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var account = new AccountEntity { Name = "Contoso" };
            var id = service.Create(account);

            service.Delete("account", id);

            Assert.Throws<System.ServiceModel.FaultException<OrganizationServiceFault>>(() =>
                service.Retrieve("account", id, new ColumnSet(true)));
        }

        [Fact]
        public void EarlyBound_QueryExpression_RetrieveMultiple_CanCast()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new AccountEntity { Name = "Contoso", Revenue = new Money(5000m) });
            service.Create(new AccountEntity { Name = "Fabrikam", Revenue = new Money(3000m) });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.Equal, "Contoso");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);

            var typed = result.Entities[0].ToEntity<AccountEntity>();
            Assert.Equal("Contoso", typed.Name);
            Assert.Equal(5000m, typed.Revenue!.Value);
        }

        [Fact]
        public void EarlyBound_WithEntityReference_RoundTrips()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var contactId = service.Create(new ContactEntity { FullName = "Alice" });

            var account = new AccountEntity
            {
                Name = "Contoso",
                PrimaryContactId = new EntityReference("contact", contactId)
            };
            var acctId = service.Create(account);

            var retrieved = service.Retrieve("account", acctId, new ColumnSet(true)).ToEntity<AccountEntity>();
            Assert.NotNull(retrieved.PrimaryContactId);
            Assert.Equal(contactId, retrieved.PrimaryContactId!.Id);
        }

        [Fact]
        public void EarlyBound_LinkEntity_CanCastResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var acctId = service.Create(new AccountEntity { Name = "Contoso" });
            service.Create(new ContactEntity
            {
                FullName = "Alice",
                ParentCustomerId = new EntityReference("account", acctId)
            });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var link = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Inner);
            link.EntityAlias = "c";
            link.Columns = new ColumnSet("fullname");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);

            // Parent entity can be cast to early-bound
            var typed = result.Entities[0].ToEntity<AccountEntity>();
            Assert.Equal("Contoso", typed.Name);

            // Aliased value from link is accessible
            var alias = result.Entities[0].GetAttributeValue<AliasedValue>("c.fullname");
            Assert.Equal("Alice", alias.Value);
        }

        [Fact]
        public void EarlyBound_FetchXml_CanCastResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new AccountEntity { Name = "Contoso", Revenue = new Money(7000m) });

            var fetchXml = @"<fetch><entity name='account'><all-attributes/></entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Single(result.Entities);
            var typed = result.Entities[0].ToEntity<AccountEntity>();
            Assert.Equal("Contoso", typed.Name);
            Assert.Equal(7000m, typed.Revenue!.Value);
        }

        // ===== 6.3 Saved Queries =====

        [Fact]
        public void UserQuery_CanBeStoredAndRetrieved()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var fetchXml = @"<fetch><entity name='account'><all-attributes/>
                <filter><condition attribute='name' operator='eq' value='Contoso'/></filter>
                </entity></fetch>";
            var queryId = service.Create(new Entity("userquery")
            {
                ["name"] = "Active Accounts",
                ["fetchxml"] = fetchXml,
                ["returnedtypecode"] = "account"
            });

            var retrieved = service.Retrieve("userquery", queryId, new ColumnSet(true));
            Assert.Equal(fetchXml, retrieved.GetAttributeValue<string>("fetchxml"));
        }

        [Fact]
        public void SavedQuery_CanBeStoredAndRetrieved()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var fetchXml = @"<fetch><entity name='contact'><all-attributes/></entity></fetch>";
            var queryId = service.Create(new Entity("savedquery")
            {
                ["name"] = "All Contacts",
                ["fetchxml"] = fetchXml,
                ["returnedtypecode"] = "contact"
            });

            var retrieved = service.Retrieve("savedquery", queryId, new ColumnSet(true));
            Assert.Equal("All Contacts", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void ExecuteSavedQuery_UserQuery_ReturnsFilteredResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            // Seed data
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam" });

            // Create user query that filters to "Contoso" only
            var fetchXml = @"<fetch><entity name='account'><all-attributes/>
                <filter><condition attribute='name' operator='eq' value='Contoso'/></filter>
                </entity></fetch>";
            var queryId = service.Create(new Entity("userquery")
            {
                ["name"] = "Contoso Only",
                ["fetchxml"] = fetchXml,
                ["returnedtypecode"] = "account"
            });

            // Execute the saved query by ID
            var result = service.ExecuteSavedQuery(queryId);

            Assert.Single(result.Entities);
            Assert.Equal("Contoso", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void ExecuteSavedQuery_SavedQuery_ReturnsResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            service.Create(new Entity("contact") { ["fullname"] = "Alice" });
            service.Create(new Entity("contact") { ["fullname"] = "Bob" });

            var fetchXml = @"<fetch><entity name='contact'><all-attributes/>
                <order attribute='fullname' descending='false'/>
                </entity></fetch>";
            var queryId = service.Create(new Entity("savedquery")
            {
                ["name"] = "All Contacts Sorted",
                ["fetchxml"] = fetchXml,
                ["returnedtypecode"] = "contact"
            });

            var result = service.ExecuteSavedQuery(queryId);

            Assert.Equal(2, result.Entities.Count);
            Assert.Equal("Alice", result.Entities[0].GetAttributeValue<string>("fullname"));
            Assert.Equal("Bob", result.Entities[1].GetAttributeValue<string>("fullname"));
        }

        [Fact]
        public void ExecuteSavedQuery_NonExistentId_Throws()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            Assert.Throws<System.ServiceModel.FaultException<OrganizationServiceFault>>(() =>
                service.ExecuteSavedQuery(Guid.NewGuid()));
        }

        [Fact]
        public void ExecuteSavedQuery_NoFetchXml_ThrowsInvalidOperation()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var queryId = service.Create(new Entity("userquery")
            {
                ["name"] = "Empty Query"
                // no fetchxml attribute
            });

            Assert.Throws<InvalidOperationException>(() =>
                service.ExecuteSavedQuery(queryId));
        }

        [Fact]
        public void ExecuteSavedQuery_WithLinkEntity_Works()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var acctId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("contact") { ["fullname"] = "Alice", ["parentcustomerid"] = new EntityReference("account", acctId) });
            service.Create(new Entity("contact") { ["fullname"] = "Bob" }); // no parent

            var fetchXml = @"<fetch><entity name='contact'><attribute name='fullname'/>
                <link-entity name='account' from='accountid' to='parentcustomerid' link-type='inner'>
                    <attribute name='name' alias='accountname'/>
                </link-entity>
                </entity></fetch>";
            var queryId = service.Create(new Entity("userquery")
            {
                ["name"] = "Contacts With Account",
                ["fetchxml"] = fetchXml,
                ["returnedtypecode"] = "contact"
            });

            var result = service.ExecuteSavedQuery(queryId);

            Assert.Single(result.Entities);
            Assert.Equal("Alice", result.Entities[0].GetAttributeValue<string>("fullname"));
        }
    }
}
