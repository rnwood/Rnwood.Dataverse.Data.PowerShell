using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class AdvancedFilteringTests
    {
        [Fact]
        public void DeepNestedFilter_AndWithinOr()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A", ["city"] = "NYC", ["employees"] = 100 });
            service.Create(new Entity("account") { ["name"] = "B", ["city"] = "LA", ["employees"] = 200 });
            service.Create(new Entity("account") { ["name"] = "C", ["city"] = "NYC", ["employees"] = 50 });
            service.Create(new Entity("account") { ["name"] = "D", ["city"] = "Chicago", ["employees"] = 500 });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            // OR( AND(city=NYC, employees>75), AND(city=LA) )
            query.Criteria.FilterOperator = LogicalOperator.Or;

            var nycFilter = new FilterExpression(LogicalOperator.And);
            nycFilter.AddCondition("city", ConditionOperator.Equal, "NYC");
            nycFilter.AddCondition("employees", ConditionOperator.GreaterThan, 75);
            query.Criteria.Filters.Add(nycFilter);

            var laFilter = new FilterExpression(LogicalOperator.And);
            laFilter.AddCondition("city", ConditionOperator.Equal, "LA");
            query.Criteria.Filters.Add(laFilter);

            var result = service.RetrieveMultiple(query);
            // A: NYC, 100 > 75 -> yes; B: LA -> yes; C: NYC, 50 !> 75 -> no; D: Chicago -> no
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void ThreeLevelDeepNesting()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A", ["city"] = "NYC", ["employees"] = 100, ["active"] = true });
            service.Create(new Entity("account") { ["name"] = "B", ["city"] = "NYC", ["employees"] = 100, ["active"] = false });
            service.Create(new Entity("account") { ["name"] = "C", ["city"] = "NYC", ["employees"] = 50, ["active"] = true });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            // AND( city=NYC, OR( AND(employees>=100, active=true), employees<60 ) )
            query.Criteria.FilterOperator = LogicalOperator.And;
            query.Criteria.AddCondition("city", ConditionOperator.Equal, "NYC");

            var orFilter = new FilterExpression(LogicalOperator.Or);

            var andSub = new FilterExpression(LogicalOperator.And);
            andSub.AddCondition("employees", ConditionOperator.GreaterEqual, 100);
            andSub.AddCondition("active", ConditionOperator.Equal, true);
            orFilter.Filters.Add(andSub);

            orFilter.AddCondition("employees", ConditionOperator.LessThan, 60);
            query.Criteria.Filters.Add(orFilter);

            var result = service.RetrieveMultiple(query);
            // A: NYC, (100>=100 AND active) -> yes via AND sub
            // B: NYC, (100>=100 BUT !active) -> no via AND, (100 !< 60) -> no via condition
            // C: NYC, (50 !>= 100) -> no via AND, (50 < 60) -> yes via condition
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void NullComparison_GreaterThan_NullReturnsFalse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["employees"] = 100 });
            service.Create(new Entity("account")); // null employees

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("employees", ConditionOperator.GreaterThan, 50);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void NullComparison_Equal_NullDoesNotMatchValue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account")); // null name

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.Equal, "A");

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void NullComparison_NotEqual_NullIncluded()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account") { ["name"] = "B" });
            service.Create(new Entity("account")); // null name

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.NotEqual, "A");

            var result = service.RetrieveMultiple(query);
            // B and null should match NotEqual "A"
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void EmptyFilter_ReturnsAll()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account") { ["name"] = "B" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria = new FilterExpression(); // empty filter

            Assert.Equal(2, service.RetrieveMultiple(query).Entities.Count);
        }

        [Fact]
        public void OrFilter_AtTopLevel()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account") { ["name"] = "B" });
            service.Create(new Entity("account") { ["name"] = "C" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.FilterOperator = LogicalOperator.Or;
            query.Criteria.AddCondition("name", ConditionOperator.Equal, "A");
            query.Criteria.AddCondition("name", ConditionOperator.Equal, "C");

            Assert.Equal(2, service.RetrieveMultiple(query).Entities.Count);
        }

        [Fact]
        public void Like_WildcardPatterns()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });
            service.Create(new Entity("account") { ["name"] = "Contoso Corp" });

            // Starts with
            var q1 = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            q1.Criteria.AddCondition("name", ConditionOperator.Like, "Contoso%");
            Assert.Equal(2, service.RetrieveMultiple(q1).Entities.Count);

            // Ends with
            var q2 = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            q2.Criteria.AddCondition("name", ConditionOperator.Like, "%Inc");
            Assert.Single(service.RetrieveMultiple(q2).Entities);

            // Contains
            var q3 = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            q3.Criteria.AddCondition("name", ConditionOperator.Like, "%oso%");
            Assert.Equal(2, service.RetrieveMultiple(q3).Entities.Count);
        }
    }
}
