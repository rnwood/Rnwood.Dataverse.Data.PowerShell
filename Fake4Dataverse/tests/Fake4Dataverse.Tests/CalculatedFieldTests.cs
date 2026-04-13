using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class CalculatedFieldTests
    {
        [Fact]
        public void CalculatedField_ComputedOnRetrieve()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.CalculatedFields.RegisterCalculatedField("contact", "fullname",
                e => $"{e.GetAttributeValue<string>("firstname")} {e.GetAttributeValue<string>("lastname")}");

            var id = service.Create(new Entity("contact")
            {
                ["firstname"] = "John",
                ["lastname"] = "Doe"
            });

            var result = service.Retrieve("contact", id, new ColumnSet(true));

            Assert.Equal("John Doe", result.GetAttributeValue<string>("fullname"));
        }

        [Fact]
        public void CalculatedField_ComputedOnRetrieveMultiple()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.CalculatedFields.RegisterCalculatedField("contact", "fullname",
                e => $"{e.GetAttributeValue<string>("firstname")} {e.GetAttributeValue<string>("lastname")}");

            service.Create(new Entity("contact") { ["firstname"] = "John", ["lastname"] = "Doe" });
            service.Create(new Entity("contact") { ["firstname"] = "Jane", ["lastname"] = "Smith" });

            var query = new QueryExpression("contact") { ColumnSet = new ColumnSet(true) };
            var results = service.RetrieveMultiple(query);

            Assert.Equal(2, results.Entities.Count);
            Assert.Contains(results.Entities, e => e.GetAttributeValue<string>("fullname") == "John Doe");
            Assert.Contains(results.Entities, e => e.GetAttributeValue<string>("fullname") == "Jane Smith");
        }

        [Fact]
        public void CalculatedField_NumericFormula()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.CalculatedFields.RegisterCalculatedField("opportunity", "totalvalue",
                e =>
                {
                    var price = e.GetAttributeValue<Money>("price");
                    var qty = e.GetAttributeValue<int>("quantity");
                    return price != null ? new Money(price.Value * qty) : null;
                });

            var id = service.Create(new Entity("opportunity")
            {
                ["price"] = new Money(100m),
                ["quantity"] = 5
            });

            var result = service.Retrieve("opportunity", id, new ColumnSet(true));
            var total = result.GetAttributeValue<Money>("totalvalue");

            Assert.NotNull(total);
            Assert.Equal(500m, total.Value);
        }

        [Fact]
        public void RollupField_SumOfRelatedEntities()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.CalculatedFields.RegisterRollupField(
                "account", "totalrevenue",
                "opportunity", "revenue",
                "parentaccountid",
                RollupType.Sum);

            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });

            service.Create(new Entity("opportunity")
            {
                ["revenue"] = new Money(1000m),
                ["parentaccountid"] = new EntityReference("account", accountId)
            });
            service.Create(new Entity("opportunity")
            {
                ["revenue"] = new Money(2000m),
                ["parentaccountid"] = new EntityReference("account", accountId)
            });

            var result = service.Retrieve("account", accountId, new ColumnSet(true));

            Assert.Equal(3000m, result.GetAttributeValue<decimal>("totalrevenue"));
        }

        [Fact]
        public void RollupField_CountOfRelatedEntities()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.CalculatedFields.RegisterRollupField(
                "account", "opportunitycount",
                "opportunity", "opportunityid",
                "parentaccountid",
                RollupType.Count);

            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });

            service.Create(new Entity("opportunity")
            {
                ["name"] = "Opp1",
                ["parentaccountid"] = new EntityReference("account", accountId)
            });
            service.Create(new Entity("opportunity")
            {
                ["name"] = "Opp2",
                ["parentaccountid"] = new EntityReference("account", accountId)
            });
            service.Create(new Entity("opportunity")
            {
                ["name"] = "Opp3",
                ["parentaccountid"] = new EntityReference("account", accountId)
            });

            var result = service.Retrieve("account", accountId, new ColumnSet(true));

            Assert.Equal(3, result.GetAttributeValue<int>("opportunitycount"));
        }

        [Fact]
        public void RollupField_AvgOfRelatedEntities()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.CalculatedFields.RegisterRollupField(
                "account", "avgrevenue",
                "opportunity", "revenue",
                "parentaccountid",
                RollupType.Avg);

            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });

            service.Create(new Entity("opportunity")
            {
                ["revenue"] = new Money(100m),
                ["parentaccountid"] = new EntityReference("account", accountId)
            });
            service.Create(new Entity("opportunity")
            {
                ["revenue"] = new Money(200m),
                ["parentaccountid"] = new EntityReference("account", accountId)
            });

            var result = service.Retrieve("account", accountId, new ColumnSet(true));

            Assert.Equal(150m, result.GetAttributeValue<decimal>("avgrevenue"));
        }

        [Fact]
        public void RollupField_WithFilter()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition("statecode", ConditionOperator.Equal, 0);

            env.CalculatedFields.RegisterRollupField(
                "account", "activerevenue",
                "opportunity", "revenue",
                "parentaccountid",
                RollupType.Sum,
                filter);

            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });

            service.Create(new Entity("opportunity")
            {
                ["revenue"] = new Money(1000m),
                ["statecode"] = new OptionSetValue(0),
                ["parentaccountid"] = new EntityReference("account", accountId)
            });
            service.Create(new Entity("opportunity")
            {
                ["revenue"] = new Money(500m),
                ["statecode"] = new OptionSetValue(1),
                ["parentaccountid"] = new EntityReference("account", accountId)
            });

            var result = service.Retrieve("account", accountId, new ColumnSet(true));

            Assert.Equal(1000m, result.GetAttributeValue<decimal>("activerevenue"));
        }

        [Fact]
        public void RollupField_NoRelatedRecords_ReturnsNull()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.CalculatedFields.RegisterRollupField(
                "account", "totalrevenue",
                "opportunity", "revenue",
                "parentaccountid",
                RollupType.Sum);

            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var result = service.Retrieve("account", accountId, new ColumnSet(true));

            Assert.Null(result["totalrevenue"]);
        }

        [Fact]
        public void RollupField_MinMax()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.CalculatedFields.RegisterRollupField(
                "account", "minrevenue",
                "opportunity", "revenue",
                "parentaccountid",
                RollupType.Min);
            env.CalculatedFields.RegisterRollupField(
                "account", "maxrevenue",
                "opportunity", "revenue",
                "parentaccountid",
                RollupType.Max);

            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });

            service.Create(new Entity("opportunity")
            {
                ["revenue"] = new Money(100m),
                ["parentaccountid"] = new EntityReference("account", accountId)
            });
            service.Create(new Entity("opportunity")
            {
                ["revenue"] = new Money(500m),
                ["parentaccountid"] = new EntityReference("account", accountId)
            });

            var result = service.Retrieve("account", accountId, new ColumnSet(true));

            Assert.Equal(100m, result.GetAttributeValue<decimal>("minrevenue"));
            Assert.Equal(500m, result.GetAttributeValue<decimal>("maxrevenue"));
        }
    }
}
