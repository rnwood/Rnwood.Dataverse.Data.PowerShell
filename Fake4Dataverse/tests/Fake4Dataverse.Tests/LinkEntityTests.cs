using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class LinkEntityTests
    {
        private FakeOrganizationService CreateSeededService()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            // Accounts
            var acct1 = new Entity("account") { Id = Guid.NewGuid(), ["name"] = "Contoso" };
            var acct2 = new Entity("account") { Id = Guid.NewGuid(), ["name"] = "Fabrikam" };
            var acct3 = new Entity("account") { Id = Guid.NewGuid(), ["name"] = "Northwind" };
            service.Create(acct1);
            service.Create(acct2);
            service.Create(acct3);

            // Contacts linked to accounts via parentcustomerid
            service.Create(new Entity("contact") { ["fullname"] = "Alice", ["parentcustomerid"] = new EntityReference("account", acct1.Id) });
            service.Create(new Entity("contact") { ["fullname"] = "Bob", ["parentcustomerid"] = new EntityReference("account", acct1.Id) });
            service.Create(new Entity("contact") { ["fullname"] = "Charlie", ["parentcustomerid"] = new EntityReference("account", acct2.Id) });
            // No contacts for Northwind

            return service;
        }

        [Fact]
        public void InnerJoin_ReturnsOnlyMatchedParents()
        {
            var service = CreateSeededService();

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var link = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Inner);
            link.Columns = new ColumnSet("fullname");
            link.EntityAlias = "c";

            var result = service.RetrieveMultiple(query);

            // Contoso has 2 contacts, Fabrikam has 1, Northwind has 0
            // Inner join: 3 rows total (Contoso×2 + Fabrikam×1)
            Assert.Equal(3, result.Entities.Count);

            // Northwind should NOT appear
            foreach (var e in result.Entities)
                Assert.NotEqual("Northwind", e.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void LeftOuterJoin_ReturnsAllParents()
        {
            var service = CreateSeededService();

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var link = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.LeftOuter);
            link.Columns = new ColumnSet("fullname");
            link.EntityAlias = "c";

            var result = service.RetrieveMultiple(query);

            // Contoso×2 + Fabrikam×1 + Northwind×1 (outer, no match) = 4 rows
            Assert.Equal(4, result.Entities.Count);

            // Northwind should appear once with no aliased contact columns
            var northwind = result.Entities.Where(e => e.GetAttributeValue<string>("name") == "Northwind").ToList();
            Assert.Single(northwind);
            Assert.False(northwind[0].Contains("c.fullname"));
        }

        [Fact]
        public void AliasedAttributes_AreProjectedCorrectly()
        {
            var service = CreateSeededService();

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var link = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Inner);
            link.Columns = new ColumnSet("fullname");
            link.EntityAlias = "ct";

            var result = service.RetrieveMultiple(query);

            foreach (var e in result.Entities)
            {
                Assert.True(e.Contains("ct.fullname"), "Should contain aliased attribute 'ct.fullname'");
                var aliased = e.GetAttributeValue<AliasedValue>("ct.fullname");
                Assert.NotNull(aliased);
                Assert.Equal("contact", aliased.EntityLogicalName);
                Assert.Equal("fullname", aliased.AttributeLogicalName);
                Assert.NotNull(aliased.Value);
            }
        }

        [Fact]
        public void LinkCriteria_FilterOnLinkedEntity()
        {
            var service = CreateSeededService();

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var link = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Inner);
            link.Columns = new ColumnSet("fullname");
            link.EntityAlias = "c";
            link.LinkCriteria.AddCondition("fullname", ConditionOperator.Equal, "Alice");

            var result = service.RetrieveMultiple(query);

            // Only Alice matches -> only Contoso with Alice
            Assert.Single(result.Entities);
            Assert.Equal("Contoso", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void NestedLinkEntity_ThreeLevelJoin()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            // account -> contact -> phonecall
            var acctId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact")
            {
                ["fullname"] = "Alice",
                ["parentcustomerid"] = new EntityReference("account", acctId)
            });
            service.Create(new Entity("phonecall")
            {
                ["subject"] = "Follow-up",
                ["regardingobjectid"] = new EntityReference("contact", contactId)
            });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var contactLink = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Inner);
            contactLink.EntityAlias = "c";
            contactLink.Columns = new ColumnSet("fullname");

            var phoneLink = contactLink.AddLink("phonecall", "contactid", "regardingobjectid", JoinOperator.Inner);
            phoneLink.EntityAlias = "pc";
            phoneLink.Columns = new ColumnSet("subject");

            var result = service.RetrieveMultiple(query);

            Assert.Single(result.Entities);
            var row = result.Entities[0];
            Assert.Equal("Contoso", row.GetAttributeValue<string>("name"));

            var contactAlias = row.GetAttributeValue<AliasedValue>("c.fullname");
            Assert.Equal("Alice", contactAlias.Value);

            var phoneAlias = row.GetAttributeValue<AliasedValue>("pc.subject");
            Assert.Equal("Follow-up", phoneAlias.Value);
        }

        [Fact]
        public void ColumnSet_OnLink_LimitsProjectedFields()
        {
            var service = CreateSeededService();

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var link = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Inner);
            link.Columns = new ColumnSet("fullname"); // Only fullname, not parentcustomerid etc.
            link.EntityAlias = "c";

            var result = service.RetrieveMultiple(query);

            foreach (var e in result.Entities)
            {
                Assert.True(e.Contains("c.fullname"));
                // Should NOT contain other contact attributes like parentcustomerid
                Assert.False(e.Contains("c.parentcustomerid"));
            }
        }

        [Fact]
        public void DefaultAlias_UsesEntityName()
        {
            var service = CreateSeededService();

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var link = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Inner);
            link.Columns = new ColumnSet("fullname");
            // No EntityAlias set -> should default to "contact"

            var result = service.RetrieveMultiple(query);

            foreach (var e in result.Entities)
            {
                Assert.True(e.Contains("contact.fullname"), "Should use entity name as default alias");
            }
        }

        [Fact]
        public void InnerJoin_NoMatchedLinkedRows_ReturnsEmpty()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Lonely Corp" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Inner);

            var result = service.RetrieveMultiple(query);

            Assert.Empty(result.Entities);
        }

        [Fact]
        public void MultipleLinkEntities_OnSameParent()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var acctId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("contact")
            {
                ["fullname"] = "Alice",
                ["parentcustomerid"] = new EntityReference("account", acctId)
            });
            service.Create(new Entity("task")
            {
                ["subject"] = "Review",
                ["regardingobjectid"] = new EntityReference("account", acctId)
            });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var contactLink = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Inner);
            contactLink.EntityAlias = "c";
            contactLink.Columns = new ColumnSet("fullname");

            var taskLink = query.AddLink("task", "accountid", "regardingobjectid", JoinOperator.Inner);
            taskLink.EntityAlias = "t";
            taskLink.Columns = new ColumnSet("subject");

            var result = service.RetrieveMultiple(query);

            // 1 account × 1 contact × 1 task = 1 row
            Assert.Single(result.Entities);
            var row = result.Entities[0];
            Assert.Equal("Contoso", row.GetAttributeValue<string>("name"));
            Assert.Equal("Alice", row.GetAttributeValue<AliasedValue>("c.fullname").Value);
            Assert.Equal("Review", row.GetAttributeValue<AliasedValue>("t.subject").Value);
        }

        [Fact]
        public void Exists_ReturnsParentWithMatch()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accId1 = service.Create(new Entity("account") { ["name"] = "HasContacts" });
            service.Create(new Entity("account") { ["name"] = "NoContacts" });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", accId1), ["lastname"] = "Smith" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Exists);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("HasContacts", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Exists_DoesNotDuplicateParent()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accId = service.Create(new Entity("account") { ["name"] = "MultipleContacts" });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", accId), ["lastname"] = "Smith" });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", accId), ["lastname"] = "Jones" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Exists);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void Exists_DoesNotAddLinkedColumns()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", accId), ["lastname"] = "Smith" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var link = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Exists);
            link.Columns = new ColumnSet("lastname");
            link.EntityAlias = "c";

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.False(result.Entities[0].Contains("c.lastname"));
        }

        [Fact]
        public void Any_BehavesSameAsExists()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accId1 = service.Create(new Entity("account") { ["name"] = "HasContacts" });
            service.Create(new Entity("account") { ["name"] = "NoContacts" });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", accId1), ["lastname"] = "Smith" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Any);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("HasContacts", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void In_BehavesSameAsExists()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accId1 = service.Create(new Entity("account") { ["name"] = "HasContacts" });
            service.Create(new Entity("account") { ["name"] = "NoContacts" });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", accId1), ["lastname"] = "Smith" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.In);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("HasContacts", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void NotAny_ReturnsParentWithoutMatch()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accId1 = service.Create(new Entity("account") { ["name"] = "HasContacts" });
            service.Create(new Entity("account") { ["name"] = "NoContacts" });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", accId1), ["lastname"] = "Smith" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var link = query.AddLink("contact", "accountid", "parentcustomerid");
            link.JoinOperator = JoinOperator.NotAny;

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("NoContacts", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void NotAny_ReturnsAllParentsWhenNoChildren()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account") { ["name"] = "B" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var link = query.AddLink("contact", "accountid", "parentcustomerid");
            link.JoinOperator = JoinOperator.NotAny;

            var result = service.RetrieveMultiple(query);
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void Exists_WithLinkCriteria_FiltersChildren()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accId1 = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var accId2 = service.Create(new Entity("account") { ["name"] = "Fabrikam" });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", accId1), ["lastname"] = "Smith" });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", accId2), ["lastname"] = "Jones" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var link = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.Exists);
            link.LinkCriteria.AddCondition("lastname", ConditionOperator.Equal, "Smith");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Contoso", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void NotAll_ReturnsParentWithNonMatchingChild()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accId1 = service.Create(new Entity("account") { ["name"] = "Mixed" });
            var accId2 = service.Create(new Entity("account") { ["name"] = "AllMatch" });
            service.Create(new Entity("account") { ["name"] = "NoChildren" });

            // Mixed: has one active and one inactive contact
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", accId1), ["statecode"] = new OptionSetValue(0) });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", accId1), ["statecode"] = new OptionSetValue(1) });

            // AllMatch: all contacts are active
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", accId2), ["statecode"] = new OptionSetValue(0) });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var link = query.AddLink("contact", "accountid", "parentcustomerid");
            link.JoinOperator = JoinOperator.NotAll;
            // LinkCriteria: statecode = 0 (active)
            link.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            var result = service.RetrieveMultiple(query);
            // Only "Mixed" should appear (has a child that does NOT match statecode=0)
            Assert.Single(result.Entities);
            Assert.Equal("Mixed", result.Entities[0].GetAttributeValue<string>("name"));
        }
    }
}
