using System;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class FetchXmlTests
    {
        [Fact]
        public void BasicFetchXml_ReturnsAllRows()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam" });

            var fetchXml = @"<fetch><entity name='account'><all-attributes/></entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void FetchXml_WithSpecificColumns()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso", ["revenue"] = new Money(100m) });

            var fetchXml = @"<fetch><entity name='account'><attribute name='name'/></entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Single(result.Entities);
            Assert.True(result.Entities[0].Contains("name"));
        }

        [Fact]
        public void FetchXml_WithFilter()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam" });

            var fetchXml = @"<fetch><entity name='account'><all-attributes/>
                <filter><condition attribute='name' operator='eq' value='Contoso'/></filter>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Single(result.Entities);
            Assert.Equal("Contoso", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void FetchXml_WithOrdering()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Bravo" });
            service.Create(new Entity("account") { ["name"] = "Alpha" });

            var fetchXml = @"<fetch><entity name='account'><all-attributes/>
                <order attribute='name' descending='false'/>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Equal("Alpha", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void FetchXml_WithTop()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            for (int i = 0; i < 10; i++)
                service.Create(new Entity("account") { ["name"] = $"Acc{i}" });

            var fetchXml = @"<fetch top='3'><entity name='account'><all-attributes/></entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Equal(3, result.Entities.Count);
        }

        [Fact]
        public void FetchXml_WithPaging()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            for (int i = 1; i <= 10; i++)
                service.Create(new Entity("account") { ["index"] = i });

            var fetchXml = @"<fetch count='5' page='1'><entity name='account'><all-attributes/>
                <order attribute='index' descending='false'/>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Equal(5, result.Entities.Count);
            Assert.True(result.MoreRecords);
        }

        [Fact]
        public void FetchXml_WithLinkEntity_InnerJoin()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var acctId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account") { ["name"] = "Lonely" });
            service.Create(new Entity("contact")
            {
                ["fullname"] = "Alice",
                ["parentcustomerid"] = new EntityReference("account", acctId)
            });

            var fetchXml = @"<fetch><entity name='account'><attribute name='name'/>
                <link-entity name='contact' from='parentcustomerid' to='accountid' alias='c'>
                    <attribute name='fullname'/>
                </link-entity>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Single(result.Entities);
            Assert.Equal("Contoso", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void FetchXml_WithLinkEntity_OuterJoin()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var acctId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account") { ["name"] = "Lonely" });
            service.Create(new Entity("contact")
            {
                ["fullname"] = "Alice",
                ["parentcustomerid"] = new EntityReference("account", acctId)
            });

            var fetchXml = @"<fetch><entity name='account'><attribute name='name'/>
                <link-entity name='contact' from='parentcustomerid' to='accountid' link-type='outer' alias='c'>
                    <attribute name='fullname'/>
                </link-entity>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void FetchXml_DirectEvaluation_NotAnyJoin_MatchesConvertedQueryResults()
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

            var fetchXml = @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <order attribute='name' />
                    <link-entity name='contact' from='parentcustomerid' to='accountid' link-type='not-any'>
                        <filter>
                            <condition attribute='fullname' operator='eq' value='John' />
                        </filter>
                    </link-entity>
                </entity>
            </fetch>";

            var convertedResult = service.RetrieveMultiple(ConvertFetchToQueryExpression(service, fetchXml));
            Assert.Equal(new[] { "Fabrikam", "Tailspin" }, convertedResult.Entities.Select(e => e.GetAttributeValue<string>("name")).ToArray());

            var directResult = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Equal(
                convertedResult.Entities.Select(e => e.GetAttributeValue<string>("name")),
                directResult.Entities.Select(e => e.GetAttributeValue<string>("name")));
        }

        [Fact]
        public void FetchXml_WithNestedFilter()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A", ["city"] = "NYC" });
            service.Create(new Entity("account") { ["name"] = "B", ["city"] = "LA" });
            service.Create(new Entity("account") { ["name"] = "C", ["city"] = "NYC" });

            var fetchXml = @"<fetch><entity name='account'><all-attributes/>
                <filter type='and'>
                    <condition attribute='city' operator='eq' value='NYC'/>
                    <filter type='or'>
                        <condition attribute='name' operator='eq' value='A'/>
                        <condition attribute='name' operator='eq' value='C'/>
                    </filter>
                </filter>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void FetchXml_NullOperator()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "HasName" });
            service.Create(new Entity("account")); // no name

            var fetchXml = @"<fetch><entity name='account'><all-attributes/>
                <filter><condition attribute='name' operator='null'/></filter>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Single(result.Entities);
        }

        [Fact]
        public void FetchXml_InOperator_FiltersMultipleValues()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account") { ["name"] = "B" });
            service.Create(new Entity("account") { ["name"] = "C" });

            var fetchXml = @"<fetch><entity name='account'>
                <attribute name='name'/>
                <filter>
                    <condition attribute='name' operator='in'>
                        <value>A</value>
                        <value>C</value>
                    </condition>
                </filter>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void FetchXml_NotInOperator_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account") { ["name"] = "B" });
            service.Create(new Entity("account") { ["name"] = "C" });

            var fetchXml = @"<fetch><entity name='account'>
                <attribute name='name'/>
                <filter>
                    <condition attribute='name' operator='not-in'>
                        <value>A</value>
                        <value>B</value>
                    </condition>
                </filter>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Assert.Single(result.Entities);
            Assert.Equal("C", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void FetchXml_DoesNotBeginWith_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam" });

            var fetchXml = @"<fetch><entity name='account'>
                <attribute name='name'/>
                <filter>
                    <condition attribute='name' operator='not-begin-with' value='Con'/>
                </filter>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Assert.Single(result.Entities);
            Assert.Equal("Fabrikam", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void FetchXml_DoesNotEndWith_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var fetchXml = @"<fetch><entity name='account'>
                <attribute name='name'/>
                <filter>
                    <condition attribute='name' operator='not-end-with' value='Ltd'/>
                </filter>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Assert.Single(result.Entities);
            Assert.Equal("Fabrikam Inc", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void FetchXml_Distinct_DeduplicatesJoinedResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var parentId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", parentId) });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", parentId) });

            var fetchXml = @"<fetch distinct='true'><entity name='account'>
                <attribute name='name'/>
                <link-entity name='contact' from='parentcustomerid' to='accountid'/>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Assert.Single(result.Entities);
        }

        [Fact]
        public void FetchXml_BeginsWithOperator()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam" });

            var fetchXml = @"<fetch><entity name='account'>
                <attribute name='name'/>
                <filter>
                    <condition attribute='name' operator='begins-with' value='Con'/>
                </filter>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Assert.Single(result.Entities);
            Assert.Equal("Contoso", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void FetchXml_EndsWithOperator()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var fetchXml = @"<fetch><entity name='account'>
                <attribute name='name'/>
                <filter>
                    <condition attribute='name' operator='ends-with' value='Inc'/>
                </filter>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Assert.Single(result.Entities);
            Assert.Equal("Fabrikam Inc", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void FetchXml_ContainsOperator()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var fetchXml = @"<fetch><entity name='account'>
                <attribute name='name'/>
                <filter>
                    <condition attribute='name' operator='like' value='%tos%'/>
                </filter>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Assert.Single(result.Entities);
            Assert.Equal("Contoso Ltd", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void FetchXml_DirectEvaluation_OlderThanXHours_MatchesConvertedQueryResults()
        {
            var env = new FakeDataverseEnvironment();
            env.Clock = new FakeClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));

            var service = env.CreateOrganizationService();
            service.Create(new Entity("task")
            {
                ["subject"] = "Old enough",
                ["scheduledend"] = new DateTime(2026, 3, 15, 8, 30, 0, DateTimeKind.Utc)
            });
            service.Create(new Entity("task")
            {
                ["subject"] = "Too recent",
                ["scheduledend"] = new DateTime(2026, 3, 15, 10, 30, 0, DateTimeKind.Utc)
            });

            var fetchXml = @"<fetch>
                <entity name='task'>
                    <attribute name='subject' />
                    <order attribute='subject' />
                    <filter>
                        <condition attribute='scheduledend' operator='older-than-x-hours' value='2' />
                    </filter>
                </entity>
            </fetch>";

            var convertedResult = service.RetrieveMultiple(ConvertFetchToQueryExpression(service, fetchXml));
            Assert.Equal(new[] { "Old enough" }, convertedResult.Entities.Select(e => e.GetAttributeValue<string>("subject")).ToArray());

            var directResult = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Equal(
                convertedResult.Entities.Select(e => e.GetAttributeValue<string>("subject")),
                directResult.Entities.Select(e => e.GetAttributeValue<string>("subject")));
        }

        [Fact]
        public void FetchXml_ColumnAlias_ReturnsAliasedValue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso", ["revenue"] = new Money(1000m) });

            var fetchXml = @"<fetch>
                <entity name='account'>
                    <attribute name='name' alias='account_name' />
                    <attribute name='revenue' alias='account_revenue' />
                </entity>
            </fetch>";

            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Assert.Single(result.Entities);
            var entity = result.Entities[0];

            // Aliased attributes should be wrapped in AliasedValue
            var nameAlias = entity.GetAttributeValue<AliasedValue>("account_name");
            Assert.NotNull(nameAlias);
            Assert.Equal("Contoso", nameAlias.Value);

            var revenueAlias = entity.GetAttributeValue<AliasedValue>("account_revenue");
            Assert.NotNull(revenueAlias);
            Assert.IsType<Money>(revenueAlias.Value);
            Assert.Equal(1000m, ((Money)revenueAlias.Value).Value);
        }

        [Fact]
        public void FetchXmlToQueryExpression_ConvertsBasicFetch()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam" });

            var fetchXml = @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='name' operator='eq' value='Contoso' />
                    </filter>
                </entity>
            </fetch>";

            var request = new OrganizationRequest("FetchXmlToQueryExpression");
            request["FetchXml"] = fetchXml;
            var response = service.Execute(request);
            var query = (QueryExpression)response["Query"];

            Assert.Equal("account", query.EntityName);
            Assert.Contains("name", query.ColumnSet.Columns);

            // Execute the converted query to verify it works
            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Contoso", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void ExecuteFetch_ReturnsSerializedFetchResult()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            var response = (ExecuteFetchResponse)service.Execute(new ExecuteFetchRequest
            {
                FetchXml = @"<fetch><entity name='account'><attribute name='name'/></entity></fetch>"
            });

            Assert.Contains("<resultset", response.FetchXmlResult, StringComparison.Ordinal);
            Assert.Contains("<name>Contoso</name>", response.FetchXmlResult, StringComparison.Ordinal);
        }

        [Fact]
        public void ExecuteByIdSavedQuery_ReturnsSerializedFetchResult()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Fabrikam" });
            var savedQueryId = service.Create(new Entity("savedquery")
            {
                ["name"] = "Accounts",
                ["fetchxml"] = @"<fetch><entity name='account'><attribute name='name'/></entity></fetch>"
            });

            var response = (ExecuteByIdSavedQueryResponse)service.Execute(new ExecuteByIdSavedQueryRequest
            {
                EntityId = savedQueryId
            });

            Assert.Contains("<resultset", response.String, StringComparison.Ordinal);
            Assert.Contains("<name>Fabrikam</name>", response.String, StringComparison.Ordinal);
        }

        [Fact]
        public void ExecuteByIdUserQuery_ReturnsSerializedFetchResult()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Tailspin" });
            var userQueryId = service.Create(new Entity("userquery")
            {
                ["name"] = "My Accounts",
                ["fetchxml"] = @"<fetch><entity name='account'><attribute name='name'/></entity></fetch>"
            });

            var response = (ExecuteByIdUserQueryResponse)service.Execute(new ExecuteByIdUserQueryRequest
            {
                EntityId = new EntityReference("userquery", userQueryId)
            });

            Assert.Contains("<resultset", response.String, StringComparison.Ordinal);
            Assert.Contains("<name>Tailspin</name>", response.String, StringComparison.Ordinal);
        }

        private static QueryExpression ConvertFetchToQueryExpression(IOrganizationService service, string fetchXml)
        {
            var request = new OrganizationRequest("FetchXmlToQueryExpression");
            request["FetchXml"] = fetchXml;

            var response = service.Execute(request);
            return (QueryExpression)response["Query"];
        }
    }

    public class AggregateTests
    {
        [Fact]
        public void Count_AllRows()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account") { ["name"] = "B" });
            service.Create(new Entity("account") { ["name"] = "C" });

            var fetchXml = @"<fetch aggregate='true'><entity name='account'>
                <attribute name='accountid' aggregate='count' alias='total'/>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Single(result.Entities);
            var total = result.Entities[0].GetAttributeValue<AliasedValue>("total");
            Assert.Equal(3, total.Value);
        }

        [Fact]
        public void Sum_MoneyField()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["revenue"] = new Money(100m) });
            service.Create(new Entity("account") { ["revenue"] = new Money(200m) });
            service.Create(new Entity("account") { ["revenue"] = new Money(300m) });

            var fetchXml = @"<fetch aggregate='true'><entity name='account'>
                <attribute name='revenue' aggregate='sum' alias='total_revenue'/>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            var total = result.Entities[0].GetAttributeValue<AliasedValue>("total_revenue");
            Assert.Equal(600m, total.Value);
        }

        [Fact]
        public void Avg_IntField()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["employees"] = 10 });
            service.Create(new Entity("account") { ["employees"] = 20 });
            service.Create(new Entity("account") { ["employees"] = 30 });

            var fetchXml = @"<fetch aggregate='true'><entity name='account'>
                <attribute name='employees' aggregate='avg' alias='avg_emp'/>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            var avg = result.Entities[0].GetAttributeValue<AliasedValue>("avg_emp");
            Assert.Equal(20m, avg.Value);
        }

        [Fact]
        public void Min_Max()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["employees"] = 10 });
            service.Create(new Entity("account") { ["employees"] = 50 });
            service.Create(new Entity("account") { ["employees"] = 30 });

            var fetchMin = @"<fetch aggregate='true'><entity name='account'>
                <attribute name='employees' aggregate='min' alias='min_emp'/>
                </entity></fetch>";
            var fetchMax = @"<fetch aggregate='true'><entity name='account'>
                <attribute name='employees' aggregate='max' alias='max_emp'/>
                </entity></fetch>";

            var minResult = service.RetrieveMultiple(new FetchExpression(fetchMin));
            var maxResult = service.RetrieveMultiple(new FetchExpression(fetchMax));

            Assert.Equal(10m, minResult.Entities[0].GetAttributeValue<AliasedValue>("min_emp").Value);
            Assert.Equal(50m, maxResult.Entities[0].GetAttributeValue<AliasedValue>("max_emp").Value);
        }

        [Fact]
        public void GroupBy_WithCount()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["city"] = "NYC" });
            service.Create(new Entity("account") { ["city"] = "NYC" });
            service.Create(new Entity("account") { ["city"] = "LA" });

            var fetchXml = @"<fetch aggregate='true'><entity name='account'>
                <attribute name='city' groupby='true' alias='city'/>
                <attribute name='accountid' aggregate='count' alias='cnt'/>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Equal(2, result.Entities.Count);
            var nyc = result.Entities.FirstOrDefault(e =>
                (string)e.GetAttributeValue<AliasedValue>("city").Value == "NYC");
            Assert.NotNull(nyc);
            Assert.Equal(2, nyc.GetAttributeValue<AliasedValue>("cnt").Value);
        }

        [Fact]
        public void GroupBy_WithSum()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["city"] = "NYC", ["revenue"] = new Money(100m) });
            service.Create(new Entity("account") { ["city"] = "NYC", ["revenue"] = new Money(200m) });
            service.Create(new Entity("account") { ["city"] = "LA", ["revenue"] = new Money(500m) });

            var fetchXml = @"<fetch aggregate='true'><entity name='account'>
                <attribute name='city' groupby='true' alias='city'/>
                <attribute name='revenue' aggregate='sum' alias='total'/>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Equal(2, result.Entities.Count);
            var nyc = result.Entities.FirstOrDefault(e =>
                (string)e.GetAttributeValue<AliasedValue>("city").Value == "NYC");
            Assert.Equal(300m, nyc!.GetAttributeValue<AliasedValue>("total").Value);
        }

        [Fact]
        public void Aggregate_WithFilter()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["city"] = "NYC", ["revenue"] = new Money(100m) });
            service.Create(new Entity("account") { ["city"] = "LA", ["revenue"] = new Money(200m) });

            var fetchXml = @"<fetch aggregate='true'><entity name='account'>
                <attribute name='revenue' aggregate='sum' alias='total'/>
                <filter><condition attribute='city' operator='eq' value='NYC'/></filter>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Equal(100m, result.Entities[0].GetAttributeValue<AliasedValue>("total").Value);
        }

        [Fact]
        public void CountColumn_SkipsNulls()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account") { ["name"] = "B" });
            service.Create(new Entity("account")); // no name

            var fetchXml = @"<fetch aggregate='true'><entity name='account'>
                <attribute name='name' aggregate='countcolumn' alias='cnt'/>
                </entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Equal(2, result.Entities[0].GetAttributeValue<AliasedValue>("cnt").Value);
        }
    }
}
