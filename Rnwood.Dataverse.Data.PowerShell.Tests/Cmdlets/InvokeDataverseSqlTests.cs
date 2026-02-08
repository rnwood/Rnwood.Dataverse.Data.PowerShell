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
        [Fact(Skip = "Requires TDS endpoint support - run as E2E test")]
        public void InvokeDataverseSql_DateManipulationQuery_SupportsDateFunctions()
        {
            // This test validates that date functions work with TDS endpoint
            // FakeXrmEasy doesn't support TDS endpoint, so this must run as E2E test

            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            var sql = @"
DECLARE @QuarterStart DATE = DATEADD(QUARTER, DATEDIFF(QUARTER, 0, GETDATE()) - 1, 0);
SELECT @QuarterStart as QuarterStart;
";

            // Act
            ps.AddCommand("Invoke-DataverseSql")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Sql", sql)
                .AddParameter("UseTdsEndpoint", true);

            ps.Invoke();

            // Assert - Should not throw "Operand type clash: int is incompatible with datetimeoffset"
            ps.HadErrors.Should().BeFalse();
        }

        // ===== SELECT Operation Tests =====

        [Fact(Skip = "SQL4Cds requires fully initialized ServiceClient that FakeXrmEasy cannot provide - run as E2E test")]
        public void InvokeDataverseSql_SelectQuery_ReturnsPSObjectsWithCorrectProperties()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseRecord", typeof(SetDataverseRecordCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Create test data
            var contact1 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "John",
                ["lastname"] = "Doe",
                ["emailaddress1"] = "john@example.com"
            };
            Service!.Create(contact1);

            var contact2 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Jane",
                ["lastname"] = "Smith",
                ["emailaddress1"] = "jane@example.com"
            };
            Service.Create(contact2);

            // Act - Execute SELECT query
            var sql = "SELECT firstname, lastname, emailaddress1 FROM contact WHERE lastname = 'Doe'";

            ps.AddCommand("Invoke-DataverseSql")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Sql", sql);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);

            var result = results[0];
            result.Properties["firstname"].Value.Should().Be("John");
            result.Properties["lastname"].Value.Should().Be("Doe");
            result.Properties["emailaddress1"].Value.Should().Be("john@example.com");
        }

        [Fact(Skip = "SQL4Cds requires fully initialized ServiceClient that FakeXrmEasy cannot provide - run as E2E test")]
        public void InvokeDataverseSql_SelectWithTop_ReturnsLimitedResults()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Create 5 test records
            for (int i = 1; i <= 5; i++)
            {
                Service!.Create(new Entity("contact")
                {
                    Id = Guid.NewGuid(),
                    ["firstname"] = $"User{i}",
                    ["lastname"] = "Test"
                });
            }

            // Act - Execute SELECT with TOP
            var sql = "SELECT TOP 2 firstname, lastname FROM contact";

            ps.AddCommand("Invoke-DataverseSql")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Sql", sql);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);

            // Verify all 5 records still exist
            var allContacts = Service!.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("contactid") });
            allContacts.Entities.Should().HaveCount(5);
        }

        [Fact(Skip = "SQL4Cds requires fully initialized ServiceClient that FakeXrmEasy cannot provide - run as E2E test")]
        public void InvokeDataverseSql_ParameterizedSelectQuery_BindsParametersCorrectly()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Create test data
            Service!.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Alice", ["lastname"] = "Johnson" });
            Service.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Bob", ["lastname"] = "Johnson" });
            Service.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Charlie", ["lastname"] = "Brown" });

            // Act - Execute parameterized query
            var sql = "SELECT firstname, lastname FROM contact WHERE lastname = @searchLastname";

            ps.AddScript(@"
                param($connection, $sql)
                $params = @{ searchLastname = 'Johnson' }
                Invoke-DataverseSql -Connection $connection -Sql $sql -Parameters $params
            ")
            .AddParameter("connection", mockConnection)
            .AddParameter("sql", sql);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);

            var allContacts = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("contactid") });
            allContacts.Entities.Should().HaveCount(3);
        }

        // ===== INSERT Operation Tests =====

        [Fact(Skip = "SQL4Cds requires fully initialized ServiceClient that FakeXrmEasy cannot provide - run as E2E test")]
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

            var mockConnection = CreateMockConnection("contact");

            // Act - Execute INSERT
            var sql = "INSERT INTO contact (firstname, lastname, emailaddress1) VALUES ('Test', 'User', 'test@example.com')";

            ps.AddCommand("Invoke-DataverseSql")
                .AddParameter("Connection", mockConnection)
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

        [Fact(Skip = "SQL4Cds requires fully initialized ServiceClient that FakeXrmEasy cannot provide - run as E2E test")]
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

            var mockConnection = CreateMockConnection("contact");

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
            .AddParameter("connection", mockConnection)
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

        [Fact(Skip = "SQL4Cds requires fully initialized ServiceClient that FakeXrmEasy cannot provide - run as E2E test")]
        public void InvokeDataverseSql_UpdateStatement_ModifiesRecords()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Create initial records
            Service!.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Original1", ["lastname"] = "UpdateTest" });
            Service.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Original2", ["lastname"] = "UpdateTest" });
            Service.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "NoChange", ["lastname"] = "Different" });

            // Act - Execute UPDATE
            var sql = "UPDATE contact SET firstname = 'Updated' WHERE lastname = 'UpdateTest'";

            ps.AddCommand("Invoke-DataverseSql")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Sql", sql);

            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();

            // Verify records were updated
            var updatedQuery = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("lastname", ConditionOperator.Equal, "UpdateTest")
                    }
                }
            };

            var updatedRecords = Service.RetrieveMultiple(updatedQuery);
            updatedRecords.Entities.Should().HaveCount(2);
            updatedRecords.Entities.Should().AllSatisfy(e => e.GetAttributeValue<string>("firstname").Should().Be("Updated"));

            // Verify unrelated record unchanged
            var unchangedQuery = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("lastname", ConditionOperator.Equal, "Different")
                    }
                }
            };

            var unchangedRecords = Service.RetrieveMultiple(unchangedQuery);
            unchangedRecords.Entities.Should().HaveCount(1);
            unchangedRecords.Entities[0].GetAttributeValue<string>("firstname").Should().Be("NoChange");
        }

        [Fact(Skip = "SQL4Cds requires fully initialized ServiceClient that FakeXrmEasy cannot provide - run as E2E test")]
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

            var mockConnection = CreateMockConnection("contact");

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
            .AddParameter("connection", mockConnection)
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

        // ===== DELETE Operation Tests =====

        [Fact(Skip = "SQL4Cds requires fully initialized ServiceClient that FakeXrmEasy cannot provide - run as E2E test")]
        public void InvokeDataverseSql_DeleteStatement_RemovesRecords()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Create test records
            Service!.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Keep1", ["lastname"] = "KeepMe" });
            Service.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Delete1", ["lastname"] = "DeleteMe" });
            Service.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Delete2", ["lastname"] = "DeleteMe" });
            Service.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Keep2", ["lastname"] = "KeepMe" });

            // Act - Execute DELETE
            var sql = "DELETE FROM contact WHERE lastname = 'DeleteMe'";

            ps.AddCommand("Invoke-DataverseSql")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Sql", sql);

            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();

            // Verify deleted records are gone
            var deletedQuery = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("contactid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("lastname", ConditionOperator.Equal, "DeleteMe")
                    }
                }
            };

            var deletedRecords = Service.RetrieveMultiple(deletedQuery);
            deletedRecords.Entities.Should().BeEmpty();

            // Verify kept records still exist
            var keptQuery = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("contactid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("lastname", ConditionOperator.Equal, "KeepMe")
                    }
                }
            };

            var keptRecords = Service.RetrieveMultiple(keptQuery);
            keptRecords.Entities.Should().HaveCount(2);
        }

        [Fact(Skip = "SQL4Cds requires fully initialized ServiceClient that FakeXrmEasy cannot provide - run as E2E test")]
        public void InvokeDataverseSql_DeleteWithWhatIf_DoesNotDeleteRecords()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Create test records
            Service!.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Test1", ["lastname"] = "WhatIfTest" });
            Service.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Test2", ["lastname"] = "WhatIfTest" });

            // Act - Execute DELETE with -WhatIf
            var sql = "DELETE FROM contact WHERE lastname = 'WhatIfTest'";

            ps.AddCommand("Invoke-DataverseSql")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Sql", sql)
                .AddParameter("WhatIf", true);

            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();

            // Verify NO records were deleted (WhatIf prevents deletion)
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname", "lastname")
            };

            var allRecords = Service.RetrieveMultiple(query);
            allRecords.Entities.Should().HaveCount(2);
            allRecords.Entities.Should().AllSatisfy(e => e.GetAttributeValue<string>("lastname").Should().Be("WhatIfTest"));
        }

        // ===== Pipeline Parameterization Tests =====

        [Fact(Skip = "SQL4Cds requires fully initialized ServiceClient that FakeXrmEasy cannot provide - run as E2E test")]
        public void InvokeDataverseSql_PipelineParameterization_ExecutesQueryOncePerPipelineObject()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Create test records
            Service!.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "User1", ["lastname"] = "Smith" });
            Service.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "User2", ["lastname"] = "Johnson" });
            Service.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "User3", ["lastname"] = "Smith" });

            // Act - Execute pipeline parameterization
            var sql = "SELECT firstname, lastname FROM contact WHERE lastname = @searchName";

            ps.AddScript(@"
                param($connection, $sql)
                $paramObjects = @(
                    @{ searchName = 'Smith' }
                    @{ searchName = 'Johnson' }
                )
                $paramObjects | Invoke-DataverseSql -Connection $connection -Sql $sql
            ")
            .AddParameter("connection", mockConnection)
            .AddParameter("sql", sql);

            var results = ps.Invoke();

            // Assert - Smith query returns 2, Johnson returns 1, total 3
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(3);

            // Verify both lastnames present
            var allRecords = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("contactid") });
            allRecords.Entities.Should().HaveCount(3);
        }
    }
}
