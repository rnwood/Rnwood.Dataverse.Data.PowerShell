using FluentAssertions;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Set-DataverseRelationshipMetadata cmdlet
    /// Migrated from tests/Set-DataverseRelationshipMetadata.Tests.ps1 (3 tests)
    /// </summary>
    public class SetDataverseRelationshipMetadataTests : TestBase
    {
        // ===== Parameter Validation ===== (2 tests)

        [Fact]
        public void SetDataverseRelationshipMetadata_HasIntersectEntitySchemaNameParameter()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseRelationshipMetadata", typeof(Commands.SetDataverseRelationshipMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Act - Get command info
            ps.AddCommand("Get-Command")
              .AddParameter("Name", "Set-DataverseRelationshipMetadata");

            var results = ps.Invoke();

            // Assert - Should have IntersectEntitySchemaName parameter
            results.Should().ContainSingle();
            var commandInfo = results[0].BaseObject as CommandInfo;
            commandInfo.Should().NotBeNull();

            var param = commandInfo!.Parameters["IntersectEntitySchemaName"];
            param.Should().NotBeNull();
            param.ParameterType.Name.Should().Be("String");
        }

        [Fact]
        public void SetDataverseRelationshipMetadata_HasIntersectEntityNameAlias()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseRelationshipMetadata", typeof(Commands.SetDataverseRelationshipMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Act - Get command info
            ps.AddCommand("Get-Command")
              .AddParameter("Name", "Set-DataverseRelationshipMetadata");

            var results = ps.Invoke();

            // Assert - Should have IntersectEntityName as alias
            results.Should().ContainSingle();
            var commandInfo = results[0].BaseObject as CommandInfo;
            commandInfo.Should().NotBeNull();

            var param = commandInfo!.Parameters["IntersectEntitySchemaName"];
            param.Should().NotBeNull();
            param.Aliases.Should().Contain("IntersectEntityName");
        }

        // ===== Create ManyToMany Relationship ===== (1 test)

        [Fact]
        public void SetDataverseRelationshipMetadata_CreateManyToMany_UsesIntersectEntitySchemaName()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseRelationshipMetadata", typeof(Commands.SetDataverseRelationshipMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection(new[] { "contact", "account" });

            // Act - Create ManyToMany relationship using IntersectEntitySchemaName parameter
            ps.AddCommand("Set-DataverseRelationshipMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("SchemaName", "new_account_contact")
              .AddParameter("RelationshipType", "ManyToMany")
              .AddParameter("ReferencedEntity", "account")
              .AddParameter("ReferencingEntity", "contact")
              .AddParameter("IntersectEntitySchemaName", "new_accountcontact")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should execute without errors
            // Note: FakeXrmEasy doesn't support CreateManyToManyRequest
            // This test validates compilation and parameter acceptance only
            // Full validation requires E2E test with real Dataverse
            ps.HadErrors.Should().BeFalse();
        }
    }
}
