using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class QueryExpressionTests
    {
        [Fact]
        public void RetrieveMultiple_ReturnsAllEntities()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            var result = service.RetrieveMultiple(query);

            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void RetrieveMultiple_WithEqualFilter_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.Equal, "Contoso");

            var result = service.RetrieveMultiple(query);

            Assert.Single(result.Entities);
            Assert.Equal("Contoso", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void RetrieveMultiple_WithLikeFilter_MatchesWildcard()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.Like, "Con%");

            var result = service.RetrieveMultiple(query);

            Assert.Single(result.Entities);
        }

        [Fact]
        public void RetrieveMultiple_WithInFilter_MatchesMultipleValues()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["statecode"] = new OptionSetValue(0) });
            service.Create(new Entity("account") { ["statecode"] = new OptionSetValue(1) });
            service.Create(new Entity("account") { ["statecode"] = new OptionSetValue(2) });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("statecode", ConditionOperator.In, 0, 2);

            var result = service.RetrieveMultiple(query);

            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void RetrieveMultiple_WithNullFilter_MatchesNullAttributes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account")); // no name attribute

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.Null);

            var result = service.RetrieveMultiple(query);

            Assert.Single(result.Entities);
        }

        [Fact]
        public void RetrieveMultiple_WithTopCount_LimitsResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            for (int i = 0; i < 10; i++)
                service.Create(new Entity("account") { ["name"] = $"Account {i}" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true), TopCount = 3 };
            var result = service.RetrieveMultiple(query);

            Assert.Equal(3, result.Entities.Count);
        }

        [Fact]
        public void RetrieveMultiple_WithOrdering_SortsResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Charlie" });
            service.Create(new Entity("account") { ["name"] = "Alice" });
            service.Create(new Entity("account") { ["name"] = "Bob" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.AddOrder("name", OrderType.Ascending);

            var result = service.RetrieveMultiple(query);

            Assert.Equal("Alice", result.Entities[0].GetAttributeValue<string>("name"));
            Assert.Equal("Bob", result.Entities[1].GetAttributeValue<string>("name"));
            Assert.Equal("Charlie", result.Entities[2].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void RetrieveMultiple_OnlyReturnsMatchingEntityType()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("contact") { ["lastname"] = "Doe" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            var result = service.RetrieveMultiple(query);

            Assert.Single(result.Entities);
            Assert.Equal("account", result.Entities[0].LogicalName);
        }

        [Fact]
        public void RetrieveMultiple_WithColumnSet_ProjectsColumns()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso", ["revenue"] = new Money(1000m) });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet("name") };
            var result = service.RetrieveMultiple(query);

            Assert.True(result.Entities[0].Contains("name"));
            Assert.False(result.Entities[0].Contains("revenue"));
        }

        [Fact]
        public void RetrieveMultiple_Distinct_DeduplicatesJoinedResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var parentId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", parentId) });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", parentId) });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true), Distinct = true };
            query.AddLink("contact", "accountid", "parentcustomerid");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void RetrieveMultiple_WithoutDistinct_AllowsDuplicateJoinedResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var parentId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", parentId) });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", parentId) });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true), Distinct = false };
            query.AddLink("contact", "accountid", "parentcustomerid");

            var result = service.RetrieveMultiple(query);
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void RetrieveMultiple_TotalRecordCount_SetCorrectlyWhenNoMoreRecords()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account") { ["name"] = "B" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            var result = service.RetrieveMultiple(query);

            Assert.Equal(2, result.TotalRecordCount);
            Assert.False(result.MoreRecords);
        }

        [Fact]
        public void RetrieveMultiple_TotalRecordCount_NegativeOneWhenMoreRecords()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            for (int i = 0; i < 5; i++)
                service.Create(new Entity("account") { ["name"] = $"Acct{i}" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.PageInfo = new PagingInfo { Count = 2, PageNumber = 1 };

            var result = service.RetrieveMultiple(query);
            Assert.Equal(-1, result.TotalRecordCount);
            Assert.True(result.MoreRecords);
        }

        [Fact]
        public void RetrieveMultiple_EqualFilter_CaseInsensitive()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.Equal, "contoso");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void RetrieveMultiple_IndexedEqualFilter_CaseInsensitiveStringMatch_MatchesFullScanResults()
        {
            var indexedEnv = new FakeDataverseEnvironment();
            indexedEnv.AddIndex("account", "name");
            var indexedService = indexedEnv.CreateOrganizationService();

            var plainEnv = new FakeDataverseEnvironment();
            var plainService = plainEnv.CreateOrganizationService();

            var contosoId = Guid.NewGuid();
            var fabrikamId = Guid.NewGuid();

            indexedService.Create(new Entity("account", contosoId) { ["name"] = "Contoso" });
            indexedService.Create(new Entity("account", fabrikamId) { ["name"] = "Fabrikam" });

            plainService.Create(new Entity("account", contosoId) { ["name"] = "Contoso" });
            plainService.Create(new Entity("account", fabrikamId) { ["name"] = "Fabrikam" });

            var indexedQuery = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            indexedQuery.Criteria.AddCondition("name", ConditionOperator.Equal, "contoso");

            var plainQuery = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            plainQuery.Criteria.AddCondition("name", ConditionOperator.Equal, "contoso");

            var indexedResult = indexedService.RetrieveMultiple(indexedQuery);
            var plainResult = plainService.RetrieveMultiple(plainQuery);

            Assert.Single(plainResult.Entities);
            Assert.Equal(plainResult.Entities.Count, indexedResult.Entities.Count);
            Assert.Equal(plainResult.Entities[0].Id, indexedResult.Entities[0].Id);
        }

        [Fact]
        public void RetrieveMultiple_AddIndexAfterSeed_CaseInsensitiveStringMatch_MatchesFullScanResults()
        {
            var indexedEnv = new FakeDataverseEnvironment();
            var indexedService = indexedEnv.CreateOrganizationService();

            var plainEnv = new FakeDataverseEnvironment();
            var plainService = plainEnv.CreateOrganizationService();

            var contosoId = Guid.NewGuid();
            var fabrikamId = Guid.NewGuid();

            indexedService.Create(new Entity("account", contosoId) { ["name"] = "Contoso" });
            indexedService.Create(new Entity("account", fabrikamId) { ["name"] = "Fabrikam" });
            indexedEnv.AddIndex("account", "name");

            plainService.Create(new Entity("account", contosoId) { ["name"] = "Contoso" });
            plainService.Create(new Entity("account", fabrikamId) { ["name"] = "Fabrikam" });

            var indexedQuery = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            indexedQuery.Criteria.AddCondition("name", ConditionOperator.Equal, "CONTOSO");

            var plainQuery = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            plainQuery.Criteria.AddCondition("name", ConditionOperator.Equal, "CONTOSO");

            var indexedResult = indexedService.RetrieveMultiple(indexedQuery);
            var plainResult = plainService.RetrieveMultiple(plainQuery);

            Assert.Single(plainResult.Entities);
            Assert.Equal(plainResult.Entities.Count, indexedResult.Entities.Count);
            Assert.Equal(plainResult.Entities[0].Id, indexedResult.Entities[0].Id);
        }

        [Fact]
        public void RetrieveMultiple_NullOrCombination_ReturnsRecordsMatchingEitherCondition()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso", ["city"] = "Seattle" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam" }); // city is null
            service.Create(new Entity("account") { ["city"] = "Portland" }); // name is null

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.FilterOperator = LogicalOperator.Or;
            query.Criteria.AddCondition("name", ConditionOperator.Equal, "Contoso");
            query.Criteria.AddCondition("city", ConditionOperator.Null);

            var result = service.RetrieveMultiple(query);
            // Contoso matches name=Contoso, Fabrikam matches city=null
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void RetrieveMultiple_NullAndCombination_ReturnsRecordsMatchingBothConditions()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso", ["city"] = "Seattle" });
            service.Create(new Entity("account") { ["name"] = "Contoso" }); // city is null
            service.Create(new Entity("account") { ["name"] = "Fabrikam", ["city"] = "Portland" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.FilterOperator = LogicalOperator.And;
            query.Criteria.AddCondition("name", ConditionOperator.Equal, "Contoso");
            query.Criteria.AddCondition("city", ConditionOperator.Null);

            var result = service.RetrieveMultiple(query);
            // Only the second record matches both conditions
            Assert.Single(result.Entities);
        }

        [Fact]
        public void Retrieve_EntityReferenceName_PopulatedFromRelatedEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact")
            {
                ["firstname"] = "John",
                ["parentcustomerid"] = new EntityReference("account", accountId)
            });

            var contact = service.Retrieve("contact", contactId, new ColumnSet("parentcustomerid"));

            var lookup = contact.GetAttributeValue<EntityReference>("parentcustomerid");
            Assert.NotNull(lookup);
            Assert.Equal("Contoso", lookup.Name);
        }
    }
}
