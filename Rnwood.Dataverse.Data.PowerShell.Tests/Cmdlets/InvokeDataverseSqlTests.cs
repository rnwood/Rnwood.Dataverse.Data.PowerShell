using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Invoke-DataverseSql cmdlet.
    /// Migrated from Pester tests in tests/Invoke-DataverseSql*.Tests.ps1 (13 tests)
    /// SQL4Cds converts SQL to QueryExpression/FetchXML which FakeXrmEasy can handle.
    /// </summary>
    public class InvokeDataverseSqlTests : TestBase
    {

        // ===== INSERT Operation Tests =====

        [Fact]
        public void InvokeDataverseSql_InsertStatement_CreatesRecord()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            CreateMockConnection("contact");
            var sql4CdsConnection = MockSql4CdsServiceClientFactory.CreateForSql4Cds(Service!);

            // Act - Execute INSERT
            var sql = "INSERT INTO contact (firstname, lastname, emailaddress1) VALUES ('Test', 'User', 'test@example.com')";

            ps.AddCommand("Invoke-DataverseSql")
                .AddParameter("Connection", sql4CdsConnection)
                .AddParameter("Sql", sql);

            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();

            // Verify record was created
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname", "lastname", "emailaddress1"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("lastname", ConditionOperator.Equal, "User")
                    }
                }
            };

            var results = Service!.RetrieveMultiple(query);
            results.Entities.Should().HaveCount(1);
            results.Entities[0].GetAttributeValue<string>("firstname").Should().Be("Test");
            results.Entities[0].GetAttributeValue<string>("emailaddress1").Should().Be("test@example.com");
        }

        [Fact]
        public void InvokeDataverseSql_ParameterizedInsertStatement_CreatesRecordWithBoundParameters()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            CreateMockConnection("contact");
            var sql4CdsConnection = MockSql4CdsServiceClientFactory.CreateForSql4Cds(Service!);

            // Act - Execute parameterized INSERT
            var sql = "INSERT INTO contact (firstname, lastname, emailaddress1) VALUES (@fname, @lname, @email)";

            ps.AddScript(@"
                param($connection, $sql)
                $params = @{ 
                    fname = 'Parameterized'
                    lname = 'Insert'
                    email = 'param@example.com'
                }
                Invoke-DataverseSql -Connection $connection -Sql $sql -Parameters $params
            ")
            .AddParameter("connection", sql4CdsConnection)
            .AddParameter("sql", sql);

            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();

            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname", "emailaddress1"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("lastname", ConditionOperator.Equal, "Insert")
                    }
                }
            };

            var results = Service!.RetrieveMultiple(query);
            results.Entities.Should().HaveCount(1);
            results.Entities[0].GetAttributeValue<string>("firstname").Should().Be("Parameterized");
        }

        // ===== UPDATE Operation Tests =====

        [Fact]
        public void InvokeDataverseSql_ParameterizedUpdateWithWhereClause_UpdatesSpecificRecord()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            CreateMockConnection("contact");
            var sql4CdsConnection = MockSql4CdsServiceClientFactory.CreateForSql4Cds(Service!);

            // Create test record
            var recordId = Guid.NewGuid();
            Service!.Create(new Entity("contact")
            {
                Id = recordId,
                ["firstname"] = "OldName",
                ["lastname"] = "UpdateMe",
                ["emailaddress1"] = "old@example.com"
            });

            // Act - Execute parameterized UPDATE
            var sql = "UPDATE contact SET firstname = @newFirstName, emailaddress1 = @newEmail WHERE contactid = @targetId";

            ps.AddScript(@"
                param($connection, $sql, $recordId)
                $params = @{
                    newFirstName = 'NewName'
                    newEmail = 'new@example.com'
                    targetId = $recordId
                }
                Invoke-DataverseSql -Connection $connection -Sql $sql -Parameters $params
            ")
            .AddParameter("connection", sql4CdsConnection)
            .AddParameter("sql", sql)
            .AddParameter("recordId", recordId);

            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();

            var updated = Service.Retrieve("contact", recordId, new ColumnSet(true));
            updated.GetAttributeValue<string>("firstname").Should().Be("NewName");
            updated.GetAttributeValue<string>("emailaddress1").Should().Be("new@example.com");
            updated.GetAttributeValue<string>("lastname").Should().Be("UpdateMe");
        }
    }
}
