using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class FetchXmlToQueryExpressionTests
    {
        private static QueryExpression Convert(IOrganizationService service, string fetchXml)
        {
            var request = new OrganizationRequest("FetchXmlToQueryExpression");
            request["FetchXml"] = fetchXml;
            var response = service.Execute(request);
            return (QueryExpression)response["Query"];
        }

        #region Basic Structure

        [Fact]
        public void Convert_BasicEntityWithColumns_SetsEntityNameAndColumns()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <attribute name='revenue' />
                </entity>
            </fetch>");

            Assert.Equal("account", query.EntityName);
            Assert.Contains("name", query.ColumnSet.Columns);
            Assert.Contains("revenue", query.ColumnSet.Columns);
            Assert.False(query.ColumnSet.AllColumns);
        }

        [Fact]
        public void Convert_AllAttributes_SetsAllColumnsTrue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <all-attributes />
                </entity>
            </fetch>");

            Assert.True(query.ColumnSet.AllColumns);
        }

        [Fact]
        public void Convert_NoAttributes_DefaultsToAllColumns()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                </entity>
            </fetch>");

            Assert.True(query.ColumnSet.AllColumns);
        }

        [Fact]
        public void Convert_MissingFetchXml_Throws()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var request = new OrganizationRequest("FetchXmlToQueryExpression");
            request["FetchXml"] = "";
            Assert.Throws<ArgumentException>(() => service.Execute(request));
        }

        [Fact]
        public void Convert_MissingEntityElement_Throws()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var request = new OrganizationRequest("FetchXmlToQueryExpression");
            request["FetchXml"] = "<fetch></fetch>";
            Assert.ThrowsAny<Exception>(() => service.Execute(request));
        }

        [Fact]
        public void Convert_Aggregate_ThrowsNotSupported()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var request = new OrganizationRequest("FetchXmlToQueryExpression");
            request["FetchXml"] = @"<fetch aggregate='true'>
                <entity name='account'>
                    <attribute name='accountid' aggregate='count' alias='total'/>
                </entity>
            </fetch>";
            Assert.Throws<NotSupportedException>(() => service.Execute(request));
        }

        #endregion

        #region Ordering

        [Fact]
        public void Convert_OrderAscending_SetsCorrectOrder()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <order attribute='name' />
                </entity>
            </fetch>");

            Assert.Single(query.Orders);
            Assert.Equal("name", query.Orders[0].AttributeName);
            Assert.Equal(OrderType.Ascending, query.Orders[0].OrderType);
        }

        [Fact]
        public void Convert_OrderDescending_SetsCorrectOrder()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <order attribute='name' descending='true' />
                </entity>
            </fetch>");

            Assert.Single(query.Orders);
            Assert.Equal(OrderType.Descending, query.Orders[0].OrderType);
        }

        [Fact]
        public void Convert_MultipleOrders_PreservesAll()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <order attribute='name' />
                    <order attribute='revenue' descending='true' />
                </entity>
            </fetch>");

            Assert.Equal(2, query.Orders.Count);
            Assert.Equal("name", query.Orders[0].AttributeName);
            Assert.Equal("revenue", query.Orders[1].AttributeName);
            Assert.Equal(OrderType.Descending, query.Orders[1].OrderType);
        }

        #endregion

        #region Top, Paging, Distinct, NoLock

        [Fact]
        public void Convert_Top_SetsTopCount()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch top='5'>
                <entity name='account'>
                    <attribute name='name' />
                </entity>
            </fetch>");

            Assert.Equal(5, query.TopCount);
        }

        [Fact]
        public void Convert_CountAndPage_SetsPagingInfo()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch count='10' page='3'>
                <entity name='account'>
                    <attribute name='name' />
                </entity>
            </fetch>");

            Assert.NotNull(query.PageInfo);
            Assert.Equal(10, query.PageInfo.Count);
            Assert.Equal(3, query.PageInfo.PageNumber);
        }

        [Fact]
        public void Convert_CountWithoutPage_DefaultsToPage1()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch count='25'>
                <entity name='account'>
                    <attribute name='name' />
                </entity>
            </fetch>");

            Assert.NotNull(query.PageInfo);
            Assert.Equal(25, query.PageInfo.Count);
            Assert.Equal(1, query.PageInfo.PageNumber);
        }

        [Fact]
        public void Convert_Distinct_SetsDistinctTrue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch distinct='true'>
                <entity name='account'>
                    <attribute name='name' />
                </entity>
            </fetch>");

            Assert.True(query.Distinct);
        }

        [Fact]
        public void Convert_NoLock_SetsNoLockTrue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch no-lock='true'>
                <entity name='account'>
                    <attribute name='name' />
                </entity>
            </fetch>");

            Assert.True(query.NoLock);
        }

        #endregion

        #region Filter Operators — Basic

        [Fact]
        public void Convert_EqOperator_ConvertsAndExecutes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam" });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='name' operator='eq' value='Contoso' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Contoso", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Convert_NeOperator_Executes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam" });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='name' operator='ne' value='Contoso' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Fabrikam", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Convert_GtLtOperators_FilterByNumericComparison()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A", ["employeescount"] = 10 });
            service.Create(new Entity("account") { ["name"] = "B", ["employeescount"] = 50 });
            service.Create(new Entity("account") { ["name"] = "C", ["employeescount"] = 100 });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='employeescount' operator='gt' value='10' />
                        <condition attribute='employeescount' operator='lt' value='100' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("B", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Convert_GeLe_IncludesBoundaries()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A", ["employeescount"] = 10 });
            service.Create(new Entity("account") { ["name"] = "B", ["employeescount"] = 50 });
            service.Create(new Entity("account") { ["name"] = "C", ["employeescount"] = 100 });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='employeescount' operator='ge' value='10' />
                        <condition attribute='employeescount' operator='le' value='100' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Equal(3, result.Entities.Count);
        }

        [Fact]
        public void Convert_NullAndNotNull_Operators()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A", ["description"] = "has desc" });
            service.Create(new Entity("account") { ["name"] = "B" }); // no description

            var queryNull = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='description' operator='null' />
                    </filter>
                </entity>
            </fetch>");

            var resultNull = service.RetrieveMultiple(queryNull);
            Assert.Single(resultNull.Entities);
            Assert.Equal("B", resultNull.Entities[0].GetAttributeValue<string>("name"));

            var queryNotNull = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='description' operator='not-null' />
                    </filter>
                </entity>
            </fetch>");

            var resultNotNull = service.RetrieveMultiple(queryNotNull);
            Assert.Single(resultNotNull.Entities);
            Assert.Equal("A", resultNotNull.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Convert_LikeOperator_MatchesPattern()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='name' operator='like' value='%Ltd' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Contoso Ltd", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Convert_NotLikeOperator_ExcludesPattern()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='name' operator='not-like' value='%Ltd' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Fabrikam Inc", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Convert_InOperator_WithChildValues()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account") { ["name"] = "B" });
            service.Create(new Entity("account") { ["name"] = "C" });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='name' operator='in'>
                            <value>A</value>
                            <value>C</value>
                        </condition>
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void Convert_NotInOperator_ExcludesValues()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account") { ["name"] = "B" });
            service.Create(new Entity("account") { ["name"] = "C" });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='name' operator='not-in'>
                            <value>A</value>
                            <value>C</value>
                        </condition>
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("B", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Convert_BeginsWithOperator_Executes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='name' operator='begins-with' value='Con' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Contoso Ltd", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Convert_EndsWithOperator_Executes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='name' operator='ends-with' value='Inc' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Fabrikam Inc", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Convert_ContainsOperator_Executes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='name' operator='contain' value='tos' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Contoso Ltd", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Convert_DoesNotBeginWithOperator_Executes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='name' operator='not-begin-with' value='Con' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Fabrikam Inc", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Convert_DoesNotEndWithOperator_Executes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='name' operator='not-end-with' value='Inc' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Contoso Ltd", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Convert_DoesNotContainOperator_Executes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='name' operator='not-contain' value='tos' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Fabrikam Inc", result.Entities[0].GetAttributeValue<string>("name"));
        }

        #endregion

        #region Range Operators

        [Fact]
        public void Convert_BetweenOperator_FiltersRange()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A", ["employeescount"] = 5 });
            service.Create(new Entity("account") { ["name"] = "B", ["employeescount"] = 50 });
            service.Create(new Entity("account") { ["name"] = "C", ["employeescount"] = 200 });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='employeescount' operator='between'>
                            <value>10</value>
                            <value>100</value>
                        </condition>
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("B", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Convert_NotBetweenOperator_ExcludesRange()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A", ["employeescount"] = 5 });
            service.Create(new Entity("account") { ["name"] = "B", ["employeescount"] = 50 });
            service.Create(new Entity("account") { ["name"] = "C", ["employeescount"] = 200 });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='employeescount' operator='not-between'>
                            <value>10</value>
                            <value>100</value>
                        </condition>
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Equal(2, result.Entities.Count);
        }

        #endregion

        #region Date Operators

        [Fact]
        public void Convert_TodayOperator_FiltersCurrentDay()
        {
            var clock = new FakeClock(new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc));
            var env = new FakeDataverseEnvironment();
            env.Clock = clock;
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["subject"] = "Today", ["createdon"] = new DateTime(2026, 3, 15, 8, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "Yesterday", ["createdon"] = new DateTime(2026, 3, 14, 8, 0, 0, DateTimeKind.Utc) });

            var query = Convert(service, @"<fetch>
                <entity name='task'>
                    <attribute name='subject' />
                    <filter>
                        <condition attribute='createdon' operator='today' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Today", result.Entities[0].GetAttributeValue<string>("subject"));
        }

        [Fact]
        public void Convert_YesterdayOperator_FiltersCorrectDay()
        {
            var clock = new FakeClock(new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc));
            var env = new FakeDataverseEnvironment();
            env.Clock = clock;
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["subject"] = "Today", ["createdon"] = new DateTime(2026, 3, 15, 8, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "Yesterday", ["createdon"] = new DateTime(2026, 3, 14, 8, 0, 0, DateTimeKind.Utc) });

            var query = Convert(service, @"<fetch>
                <entity name='task'>
                    <attribute name='subject' />
                    <filter>
                        <condition attribute='createdon' operator='yesterday' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Yesterday", result.Entities[0].GetAttributeValue<string>("subject"));
        }

        [Fact]
        public void Convert_TomorrowOperator_FiltersCorrectDay()
        {
            var clock = new FakeClock(new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc));
            var env = new FakeDataverseEnvironment();
            env.Clock = clock;
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["subject"] = "Tomorrow", ["createdon"] = new DateTime(2026, 3, 16, 8, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "Today", ["createdon"] = new DateTime(2026, 3, 15, 8, 0, 0, DateTimeKind.Utc) });

            var query = Convert(service, @"<fetch>
                <entity name='task'>
                    <attribute name='subject' />
                    <filter>
                        <condition attribute='createdon' operator='tomorrow' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Tomorrow", result.Entities[0].GetAttributeValue<string>("subject"));
        }

        [Fact]
        public void Convert_LastSevenDays_FiltersDateRange()
        {
            var clock = new FakeClock(new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc));
            var env = new FakeDataverseEnvironment();
            env.Clock = clock;
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["subject"] = "Recent", ["createdon"] = new DateTime(2026, 3, 12, 8, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "Old", ["createdon"] = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc) });

            var query = Convert(service, @"<fetch>
                <entity name='task'>
                    <attribute name='subject' />
                    <filter>
                        <condition attribute='createdon' operator='last-seven-days' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Recent", result.Entities[0].GetAttributeValue<string>("subject"));
        }

        [Fact]
        public void Convert_LastXDays_FiltersDateRange()
        {
            var clock = new FakeClock(new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc));
            var env = new FakeDataverseEnvironment();
            env.Clock = clock;
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["subject"] = "Recent", ["createdon"] = new DateTime(2026, 3, 12, 8, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "Old", ["createdon"] = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc) });

            var query = Convert(service, @"<fetch>
                <entity name='task'>
                    <attribute name='subject' />
                    <filter>
                        <condition attribute='createdon' operator='last-x-days' value='10' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Recent", result.Entities[0].GetAttributeValue<string>("subject"));
        }

        [Fact]
        public void Convert_NextXDays_FiltersDateRange()
        {
            var clock = new FakeClock(new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc));
            var env = new FakeDataverseEnvironment();
            env.Clock = clock;
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["subject"] = "Soon", ["createdon"] = new DateTime(2026, 3, 18, 8, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "Far", ["createdon"] = new DateTime(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc) });

            var query = Convert(service, @"<fetch>
                <entity name='task'>
                    <attribute name='subject' />
                    <filter>
                        <condition attribute='createdon' operator='next-x-days' value='10' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Soon", result.Entities[0].GetAttributeValue<string>("subject"));
        }

        [Fact]
        public void Convert_ThisWeek_FiltersCurrentWeek()
        {
            // 2026-03-16 is a Monday
            var clock = new FakeClock(new DateTime(2026, 3, 18, 10, 0, 0, DateTimeKind.Utc)); // Wednesday
            var env = new FakeDataverseEnvironment();
            env.Clock = clock;
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["subject"] = "ThisWeek", ["createdon"] = new DateTime(2026, 3, 17, 8, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "LastWeek", ["createdon"] = new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc) });

            var query = Convert(service, @"<fetch>
                <entity name='task'>
                    <attribute name='subject' />
                    <filter>
                        <condition attribute='createdon' operator='this-week' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("ThisWeek", result.Entities[0].GetAttributeValue<string>("subject"));
        }

        [Fact]
        public void Convert_ThisMonth_FiltersCurrentMonth()
        {
            var clock = new FakeClock(new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc));
            var env = new FakeDataverseEnvironment();
            env.Clock = clock;
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["subject"] = "ThisMonth", ["createdon"] = new DateTime(2026, 3, 5, 8, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "LastMonth", ["createdon"] = new DateTime(2026, 2, 5, 8, 0, 0, DateTimeKind.Utc) });

            var query = Convert(service, @"<fetch>
                <entity name='task'>
                    <attribute name='subject' />
                    <filter>
                        <condition attribute='createdon' operator='this-month' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("ThisMonth", result.Entities[0].GetAttributeValue<string>("subject"));
        }

        [Fact]
        public void Convert_ThisYear_FiltersCurrentYear()
        {
            var clock = new FakeClock(new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc));
            var env = new FakeDataverseEnvironment();
            env.Clock = clock;
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["subject"] = "ThisYear", ["createdon"] = new DateTime(2026, 1, 5, 8, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "LastYear", ["createdon"] = new DateTime(2025, 6, 5, 8, 0, 0, DateTimeKind.Utc) });

            var query = Convert(service, @"<fetch>
                <entity name='task'>
                    <attribute name='subject' />
                    <filter>
                        <condition attribute='createdon' operator='this-year' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("ThisYear", result.Entities[0].GetAttributeValue<string>("subject"));
        }

        [Fact]
        public void Convert_OnOperator_FiltersExactDate()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["subject"] = "Match", ["createdon"] = new DateTime(2026, 3, 15, 8, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "NoMatch", ["createdon"] = new DateTime(2026, 3, 16, 8, 0, 0, DateTimeKind.Utc) });

            var query = Convert(service, @"<fetch>
                <entity name='task'>
                    <attribute name='subject' />
                    <filter>
                        <condition attribute='createdon' operator='on' value='2026-03-15' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Match", result.Entities[0].GetAttributeValue<string>("subject"));
        }

        [Fact]
        public void Convert_OnOrBeforeOperator_Executes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["subject"] = "Before", ["createdon"] = new DateTime(2026, 3, 14, 8, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "On", ["createdon"] = new DateTime(2026, 3, 15, 8, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "After", ["createdon"] = new DateTime(2026, 3, 16, 8, 0, 0, DateTimeKind.Utc) });

            var query = Convert(service, @"<fetch>
                <entity name='task'>
                    <attribute name='subject' />
                    <filter>
                        <condition attribute='createdon' operator='on-or-before' value='2026-03-15' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void Convert_OnOrAfterOperator_Executes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["subject"] = "Before", ["createdon"] = new DateTime(2026, 3, 14, 8, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "On", ["createdon"] = new DateTime(2026, 3, 15, 8, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "After", ["createdon"] = new DateTime(2026, 3, 16, 8, 0, 0, DateTimeKind.Utc) });

            var query = Convert(service, @"<fetch>
                <entity name='task'>
                    <attribute name='subject' />
                    <filter>
                        <condition attribute='createdon' operator='on-or-after' value='2026-03-15' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void Convert_LastXHours_FiltersCorrectly()
        {
            var clock = new FakeClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            var env = new FakeDataverseEnvironment();
            env.Clock = clock;
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["subject"] = "Recent", ["createdon"] = new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "Old", ["createdon"] = new DateTime(2026, 3, 14, 8, 0, 0, DateTimeKind.Utc) });

            var query = Convert(service, @"<fetch>
                <entity name='task'>
                    <attribute name='subject' />
                    <filter>
                        <condition attribute='createdon' operator='last-x-hours' value='6' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Recent", result.Entities[0].GetAttributeValue<string>("subject"));
        }

        [Fact]
        public void Convert_OlderThanXDays_Executes()
        {
            var clock = new FakeClock(new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc));
            var env = new FakeDataverseEnvironment();
            env.Clock = clock;
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["subject"] = "Old", ["createdon"] = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc) });
            service.Create(new Entity("task") { ["subject"] = "Recent", ["createdon"] = new DateTime(2026, 3, 14, 8, 0, 0, DateTimeKind.Utc) });

            var query = Convert(service, @"<fetch>
                <entity name='task'>
                    <attribute name='subject' />
                    <filter>
                        <condition attribute='createdon' operator='older-than-x-days' value='30' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Old", result.Entities[0].GetAttributeValue<string>("subject"));
        }

        #endregion

        #region Filters — Nested and Or

        [Fact]
        public void Convert_OrFilter_MatchesEitherCondition()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account") { ["name"] = "B" });
            service.Create(new Entity("account") { ["name"] = "C" });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter type='or'>
                        <condition attribute='name' operator='eq' value='A' />
                        <condition attribute='name' operator='eq' value='C' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void Convert_NestedFilter_CombinesCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A", ["employeescount"] = 10 });
            service.Create(new Entity("account") { ["name"] = "B", ["employeescount"] = 50 });
            service.Create(new Entity("account") { ["name"] = "C", ["employeescount"] = 100 });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter type='and'>
                        <condition attribute='employeescount' operator='gt' value='5' />
                        <filter type='or'>
                            <condition attribute='name' operator='eq' value='A' />
                            <condition attribute='name' operator='eq' value='C' />
                        </filter>
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Equal(2, result.Entities.Count);
        }

        #endregion

        #region Link Entities

        [Fact]
        public void Convert_InnerJoin_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var acctId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("contact") { ["fullname"] = "John", ["parentcustomerid"] = new EntityReference("account", acctId) });
            service.Create(new Entity("account") { ["name"] = "Orphan" });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <link-entity name='contact' from='parentcustomerid' to='accountid' link-type='inner'>
                        <attribute name='fullname' />
                    </link-entity>
                </entity>
            </fetch>");

            Assert.Single(query.LinkEntities);
            Assert.Equal(JoinOperator.Inner, query.LinkEntities[0].JoinOperator);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Contoso", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Convert_OuterJoin_IncludesUnmatched()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var acctId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("contact") { ["fullname"] = "John", ["parentcustomerid"] = new EntityReference("account", acctId) });
            service.Create(new Entity("account") { ["name"] = "Orphan" });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <link-entity name='contact' from='parentcustomerid' to='accountid' link-type='outer'>
                        <attribute name='fullname' />
                    </link-entity>
                </entity>
            </fetch>");

            Assert.Equal(JoinOperator.LeftOuter, query.LinkEntities[0].JoinOperator);

            var result = service.RetrieveMultiple(query);
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void Convert_ExistsJoin_SetsCorrectJoinOperator()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <link-entity name='contact' from='parentcustomerid' to='accountid' link-type='exists' />
                </entity>
            </fetch>");

            Assert.Equal(JoinOperator.Exists, query.LinkEntities[0].JoinOperator);
        }

        [Fact]
        public void Convert_AnyJoin_SetsCorrectJoinOperator()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <link-entity name='contact' from='parentcustomerid' to='accountid' link-type='any'>
                        <filter>
                            <condition attribute='fullname' operator='eq' value='John' />
                        </filter>
                    </link-entity>
                </entity>
            </fetch>");

            Assert.Equal(JoinOperator.Any, query.LinkEntities[0].JoinOperator);
        }

        [Fact]
        public void Convert_NotAnyJoin_SetsCorrectJoinOperator()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <link-entity name='contact' from='parentcustomerid' to='accountid' link-type='not-any'>
                        <filter>
                            <condition attribute='fullname' operator='eq' value='John' />
                        </filter>
                    </link-entity>
                </entity>
            </fetch>");

            Assert.Equal(JoinOperator.NotAny, query.LinkEntities[0].JoinOperator);
        }

        [Fact]
        public void Convert_NotAllJoin_SetsCorrectJoinOperator()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <link-entity name='contact' from='parentcustomerid' to='accountid' link-type='not-all'>
                        <filter>
                            <condition attribute='fullname' operator='eq' value='John' />
                        </filter>
                    </link-entity>
                </entity>
            </fetch>");

            Assert.Equal(JoinOperator.NotAll, query.LinkEntities[0].JoinOperator);
        }

        [Fact]
        public void Convert_LinkEntityWithAlias_SetsAlias()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <link-entity name='contact' from='parentcustomerid' to='accountid' alias='c'>
                        <attribute name='fullname' />
                    </link-entity>
                </entity>
            </fetch>");

            Assert.Equal("c", query.LinkEntities[0].EntityAlias);
        }

        [Fact]
        public void Convert_LinkEntityWithAllAttributes_SetsAllColumns()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <link-entity name='contact' from='parentcustomerid' to='accountid'>
                        <all-attributes />
                    </link-entity>
                </entity>
            </fetch>");

            Assert.True(query.LinkEntities[0].Columns.AllColumns);
        }

        [Fact]
        public void Convert_LinkEntityWithFilter_SetsLinkCriteria()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <link-entity name='contact' from='parentcustomerid' to='accountid'>
                        <filter>
                            <condition attribute='statecode' operator='eq' value='0' />
                        </filter>
                    </link-entity>
                </entity>
            </fetch>");

            Assert.Single(query.LinkEntities[0].LinkCriteria.Conditions);
            Assert.Equal("statecode", query.LinkEntities[0].LinkCriteria.Conditions[0].AttributeName);
        }

        [Fact]
        public void Convert_NestedLinkEntity_ParsesRecursively()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <link-entity name='contact' from='parentcustomerid' to='accountid' alias='c'>
                        <attribute name='fullname' />
                        <link-entity name='systemuser' from='systemuserid' to='ownerid' alias='u'>
                            <attribute name='fullname' />
                        </link-entity>
                    </link-entity>
                </entity>
            </fetch>");

            Assert.Single(query.LinkEntities);
            Assert.Single(query.LinkEntities[0].LinkEntities);
            Assert.Equal("u", query.LinkEntities[0].LinkEntities[0].EntityAlias);
            Assert.Equal("systemuser", query.LinkEntities[0].LinkEntities[0].LinkToEntityName);
        }

        [Fact]
        public void Convert_UnsupportedLinkType_Throws()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            Assert.Throws<NotSupportedException>(() => Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <link-entity name='contact' from='parentcustomerid' to='accountid' link-type='cross-apply-bogus' />
                </entity>
            </fetch>"));
        }

        #endregion

        #region Condition entityname (cross-entity)

        [Fact]
        public void Convert_ConditionWithEntityName_SetsEntityNameOnCondition()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <link-entity name='contact' from='parentcustomerid' to='accountid' alias='c' />
                    <filter>
                        <condition entityname='c' attribute='fullname' operator='eq' value='John' />
                    </filter>
                </entity>
            </fetch>");

            Assert.Single(query.Criteria.Conditions);
            Assert.Equal("c", query.Criteria.Conditions[0].EntityName);
            Assert.Equal("fullname", query.Criteria.Conditions[0].AttributeName);
        }

        #endregion

        #region Value Type Conversion

        [Fact]
        public void Convert_IntegerValue_ParsedAsInt()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A", ["employeescount"] = 42 });
            service.Create(new Entity("account") { ["name"] = "B", ["employeescount"] = 10 });

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='employeescount' operator='eq' value='42' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("A", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Convert_GuidValue_ParsedAsGuid()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var targetId = Guid.NewGuid();
            service.Create(new Entity("account", targetId) { ["name"] = "Target" });
            service.Create(new Entity("account") { ["name"] = "Other" });

            var query = Convert(service, $@"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='accountid' operator='eq' value='{targetId}' />
                    </filter>
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Target", result.Entities[0].GetAttributeValue<string>("name"));
        }

        #endregion

        #region Unsupported Operator

        [Fact]
        public void Convert_UnsupportedOperator_Throws()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            Assert.Throws<NotSupportedException>(() => Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='name' operator='bogus-operator' value='test' />
                    </filter>
                </entity>
            </fetch>"));
        }

        #endregion

        #region End-to-End: Converted QE Matches FetchXml Execution

        [Fact]
        public void Convert_ComplexQuery_ProducesSameResultsAsFetchXml()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var acctId1 = service.Create(new Entity("account") { ["name"] = "Contoso", ["employeescount"] = 500 });
            var acctId2 = service.Create(new Entity("account") { ["name"] = "Fabrikam", ["employeescount"] = 100 });
            service.Create(new Entity("account") { ["name"] = "Orphan Corp", ["employeescount"] = 5 });
            service.Create(new Entity("contact") { ["fullname"] = "John", ["parentcustomerid"] = new EntityReference("account", acctId1) });
            service.Create(new Entity("contact") { ["fullname"] = "Jane", ["parentcustomerid"] = new EntityReference("account", acctId2) });

            var fetchXml = @"<fetch distinct='true'>
                <entity name='account'>
                    <attribute name='name' />
                    <attribute name='employeescount' />
                    <filter>
                        <condition attribute='employeescount' operator='gt' value='10' />
                    </filter>
                    <order attribute='name' />
                    <link-entity name='contact' from='parentcustomerid' to='accountid' link-type='inner'>
                        <attribute name='fullname' />
                    </link-entity>
                </entity>
            </fetch>";

            // Execute via FetchXml directly
            var fetchResult = service.RetrieveMultiple(new FetchExpression(fetchXml));

            // Convert and execute via QueryExpression
            var query = Convert(service, fetchXml);
            var qeResult = service.RetrieveMultiple(query);

            Assert.Equal(fetchResult.Entities.Count, qeResult.Entities.Count);
            for (int i = 0; i < fetchResult.Entities.Count; i++)
            {
                Assert.Equal(
                    fetchResult.Entities[i].GetAttributeValue<string>("name"),
                    qeResult.Entities[i].GetAttributeValue<string>("name"));
            }
        }

        [Fact]
        public void Convert_PagingQuery_ProducesCorrectPage()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            for (int i = 1; i <= 10; i++)
                service.Create(new Entity("account") { ["name"] = $"Account{i:D2}", ["sortkey"] = i });

            var query = Convert(service, @"<fetch count='3' page='2'>
                <entity name='account'>
                    <attribute name='name' />
                    <order attribute='sortkey' />
                </entity>
            </fetch>");

            var result = service.RetrieveMultiple(query);
            Assert.Equal(3, result.Entities.Count);
            Assert.Equal("Account04", result.Entities[0].GetAttributeValue<string>("name"));
            Assert.Equal("Account05", result.Entities[1].GetAttributeValue<string>("name"));
            Assert.Equal("Account06", result.Entities[2].GetAttributeValue<string>("name"));
        }

        #endregion

        #region User Context Operators

        [Fact]
        public void Convert_EqUserIdOperator_MapsCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='ownerid' operator='eq-userid' />
                    </filter>
                </entity>
            </fetch>");

            Assert.Equal(ConditionOperator.EqualUserId, query.Criteria.Conditions[0].Operator);
        }

        [Fact]
        public void Convert_NeUserIdOperator_MapsCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='ownerid' operator='ne-userid' />
                    </filter>
                </entity>
            </fetch>");

            Assert.Equal(ConditionOperator.NotEqualUserId, query.Criteria.Conditions[0].Operator);
        }

        [Fact]
        public void Convert_EqBusinessIdOperator_MapsCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var query = Convert(service, @"<fetch>
                <entity name='account'>
                    <attribute name='name' />
                    <filter>
                        <condition attribute='owningbusinessunit' operator='eq-businessid' />
                    </filter>
                </entity>
            </fetch>");

            Assert.Equal(ConditionOperator.EqualBusinessId, query.Criteria.Conditions[0].Operator);
        }

        #endregion
    }
}
