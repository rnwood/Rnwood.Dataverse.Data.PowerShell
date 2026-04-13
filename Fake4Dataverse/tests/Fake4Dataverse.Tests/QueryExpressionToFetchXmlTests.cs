using System.Linq;
using System.Xml.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class QueryExpressionToFetchXmlTests
    {
        [Fact]
        public void Convert_NotAnyJoin_PreservesJoinSemantics()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var contosoId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var fabrikamId = service.Create(new Entity("account") { ["name"] = "Fabrikam" });
            service.Create(new Entity("account") { ["name"] = "Tailspin" });

            service.Create(new Entity("contact")
            {
                ["fullname"] = "John",
                ["parentcustomerid"] = new EntityReference("account", contosoId)
            });
            service.Create(new Entity("contact")
            {
                ["fullname"] = "Jane",
                ["parentcustomerid"] = new EntityReference("account", fabrikamId)
            });

            var query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name")
            };
            query.AddOrder("name", OrderType.Ascending);

            var link = query.AddLink("contact", "accountid", "parentcustomerid", JoinOperator.NotAny);
            link.LinkCriteria.AddCondition("fullname", ConditionOperator.Equal, "John");

            var queryNames = service.RetrieveMultiple(query)
                .Entities
                .Select(entity => entity.GetAttributeValue<string>("name"))
                .ToArray();

            Assert.Equal(new[] { "Fabrikam", "Tailspin" }, queryNames);

            var fetchXml = Convert(service, query);
            var fetchDoc = XDocument.Parse(fetchXml);
            var linkType = fetchDoc.Descendants("link-entity").Single().Attribute("link-type")?.Value;

            Assert.Equal("not-any", linkType);

            var fetchNames = service.RetrieveMultiple(new FetchExpression(fetchXml))
                .Entities
                .Select(entity => entity.GetAttributeValue<string>("name"))
                .ToArray();

            Assert.Equal(queryNames, fetchNames);
        }

        [Fact]
        public void Convert_ContainsCondition_PreservesContainsSemantics()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name")
            };
            query.AddOrder("name", OrderType.Ascending);
            query.Criteria.AddCondition("name", ConditionOperator.Contains, "tos");

            var queryNames = service.RetrieveMultiple(query)
                .Entities
                .Select(entity => entity.GetAttributeValue<string>("name"))
                .ToArray();

            Assert.Equal(new[] { "Contoso Ltd" }, queryNames);

            var fetchXml = Convert(service, query);
            var fetchDoc = XDocument.Parse(fetchXml);
            var condition = fetchDoc.Descendants("condition").Single();

            Assert.Equal("contain", condition.Attribute("operator")?.Value);
            Assert.Equal("tos", condition.Attribute("value")?.Value);

            var fetchNames = service.RetrieveMultiple(new FetchExpression(fetchXml))
                .Entities
                .Select(entity => entity.GetAttributeValue<string>("name"))
                .ToArray();

            Assert.Equal(queryNames, fetchNames);
        }

        private static string Convert(IOrganizationService service, QueryExpression query)
        {
            var response = (QueryExpressionToFetchXmlResponse)service.Execute(
                new QueryExpressionToFetchXmlRequest
                {
                    Query = query
                });

            return (string)response.Results["FetchXml"];
        }
    }
}
