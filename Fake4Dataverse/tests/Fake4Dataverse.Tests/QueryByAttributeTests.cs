using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class QueryByAttributeTests
    {
        [Fact]
        public void SingleAttribute_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam" });

            var qba = new QueryByAttribute("account") { ColumnSet = new ColumnSet(true) };
            qba.AddAttributeValue("name", "Contoso");

            var result = service.RetrieveMultiple(qba);
            Assert.Single(result.Entities);
            Assert.Equal("Contoso", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void MultipleAttributes_FiltersCombined()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso", ["city"] = "NYC" });
            service.Create(new Entity("account") { ["name"] = "Contoso", ["city"] = "LA" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam", ["city"] = "NYC" });

            var qba = new QueryByAttribute("account") { ColumnSet = new ColumnSet(true) };
            qba.AddAttributeValue("name", "Contoso");
            qba.AddAttributeValue("city", "NYC");

            var result = service.RetrieveMultiple(qba);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void WithOrdering()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "B" });
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account") { ["name"] = "C" });

            var qba = new QueryByAttribute("account") { ColumnSet = new ColumnSet(true) };
            qba.AddOrder("name", OrderType.Ascending);

            var result = service.RetrieveMultiple(qba);
            Assert.Equal(3, result.Entities.Count);
            Assert.Equal("A", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void WithTopCount()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            for (int i = 0; i < 10; i++)
                service.Create(new Entity("account") { ["name"] = $"A{i}" });

            var qba = new QueryByAttribute("account")
            {
                ColumnSet = new ColumnSet(true),
                TopCount = 3
            };

            var result = service.RetrieveMultiple(qba);
            Assert.Equal(3, result.Entities.Count);
        }

        [Fact]
        public void NoMatch_ReturnsEmpty()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            var qba = new QueryByAttribute("account") { ColumnSet = new ColumnSet(true) };
            qba.AddAttributeValue("name", "DoesNotExist");

            var result = service.RetrieveMultiple(qba);
            Assert.Empty(result.Entities);
        }
    }
}
