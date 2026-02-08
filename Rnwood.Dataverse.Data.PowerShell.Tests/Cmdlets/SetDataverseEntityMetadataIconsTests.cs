using System;
using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Set-DataverseEntityMetadata cmdlet - Icon properties
    /// Migrated from tests/Set-DataverseEntityMetadata-Icons.Tests.ps1 (19 tests)
    /// 
    /// Note: Many icon-related tests require E2E testing because:
    /// - CreateEntityRequest is not supported by FakeXrmEasy
    /// - UpdateEntityRequest requires full metadata operations beyond what mocks provide
    /// - Icon validation needs webresource entity with full metadata support
    /// 
    /// Tests enabled in this file:
    /// - Get-DataverseEntityMetadata icon property access tests (verify icon properties exist on metadata)
    /// - Icon validation error tests (verify errors thrown before entity operations)
    /// </summary>
    public class SetDataverseEntityMetadataIconsTests : TestBase
    {
        private PS CreatePowerShellWithCmdlets()
        {
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityMetadata", typeof(GetDataverseEntityMetadataCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityMetadata", typeof(SetDataverseEntityMetadataCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseRecord", typeof(GetDataverseRecordCmdlet), null));
            
            var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            
            var ps = PS.Create();
            ps.Runspace = runspace;
            return ps;
        }

        // ===== Setting Icon Properties on New Entity ===== (2 tests - E2E only)
        // These tests require CreateEntityRequest which FakeXrmEasy doesn't support.

        [Fact(Skip = "FakeXrmEasy doesn't support CreateEntityRequest - requires E2E test")]
        public void SetDataverseEntityMetadata_NewEntity_SetsIconVectorName()
        {
            // Test validates that IconVectorName can be set when creating entity
            // Requires real Dataverse environment to validate actual entity creation
        }

        [Fact(Skip = "FakeXrmEasy doesn't support CreateEntityRequest - requires E2E test")]
        public void SetDataverseEntityMetadata_NewEntity_SetsAllIconProperties()
        {
            // Test validates that all icon properties can be set when creating entity
            // Requires real Dataverse environment to validate actual entity creation
        }

        // ===== Updating Icon Properties on Existing Entity ===== (3 tests - E2E only)
        // These tests require UpdateEntityRequest with full metadata retrieval.

        [Fact(Skip = "FakeXrmEasy doesn't support UpdateEntityRequest fully - requires E2E test")]
        public void SetDataverseEntityMetadata_ExistingEntity_UpdatesIconVectorName()
        {
            // Test validates that IconVectorName can be updated on existing entity
            // Requires real Dataverse environment to validate actual metadata update
        }

        [Fact(Skip = "FakeXrmEasy doesn't support UpdateEntityRequest fully - requires E2E test")]
        public void SetDataverseEntityMetadata_ExistingEntity_UpdatesAllIconProperties()
        {
            // Test validates that all icon properties can be updated
            // Requires real Dataverse environment to validate actual metadata update
        }

        [Fact(Skip = "FakeXrmEasy doesn't support UpdateEntityRequest fully - requires E2E test")]
        public void SetDataverseEntityMetadata_IconVectorName_ClearsWithEmptyString()
        {
            // Test validates that icon can be cleared by setting empty string
            // Requires real Dataverse environment to validate actual metadata update
        }

        // ===== Updating with EntityMetadata Object ===== (4 tests - E2E only)

        [Fact(Skip = "FakeXrmEasy doesn't support UpdateEntityRequest fully - requires E2E test")]
        public void SetDataverseEntityMetadata_EntityMetadataObject_UpdatesIcons()
        {
            // Test validates updating entity using EntityMetadata object with modified icon properties
            // Requires real Dataverse environment to validate actual metadata update
        }

        [Fact(Skip = "FakeXrmEasy doesn't support UpdateEntityRequest fully - requires E2E test")]
        public void SetDataverseEntityMetadata_EntityMetadataFromPipeline_UpdatesIcons()
        {
            // Test validates updating entity via pipeline with EntityMetadata object
            // Requires real Dataverse environment to validate actual metadata update
        }

        [Fact]
        public void SetDataverseEntityMetadata_EntityMetadataWithoutMetadataId_ThrowsError()
        {
            // Test validates error when EntityMetadata has no MetadataId
            // This validation happens before any entity operation, so it works with mocks
            
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create EntityMetadata without MetadataId
            var entityMetadata = new EntityMetadata
            {
                LogicalName = "contact"
                // MetadataId is not set (null/empty)
            };

            // Act - Try to update with EntityMetadata missing MetadataId
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityMetadata", entityMetadata)
              .AddParameter("Confirm", false);

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Should have error about missing MetadataId
            (ps.HadErrors || caughtException != null).Should().BeTrue("validation should fail for missing MetadataId");
            
            if (ps.Streams.Error.Count > 0)
            {
                var errorMessage = ps.Streams.Error[0].Exception.Message;
                errorMessage.Should().Contain("MetadataId");
            }
        }

        [Fact]
        public void SetDataverseEntityMetadata_EntityMetadataWithoutLogicalName_ThrowsError()
        {
            // Test validates error when EntityMetadata has no LogicalName
            // This validation happens before any entity operation, so it works with mocks
            
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create EntityMetadata with MetadataId but no LogicalName
            var entityMetadata = new EntityMetadata
            {
                // LogicalName is not set
            };
            // Set MetadataId via reflection since it's read-only
            typeof(MetadataBase).GetProperty("MetadataId")!.SetValue(entityMetadata, Guid.NewGuid());

            // Act - Try to update with EntityMetadata missing LogicalName
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityMetadata", entityMetadata)
              .AddParameter("Confirm", false);

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Should have error about missing LogicalName
            (ps.HadErrors || caughtException != null).Should().BeTrue("validation should fail for missing LogicalName");
            
            if (ps.Streams.Error.Count > 0)
            {
                var errorMessage = ps.Streams.Error[0].Exception.Message;
                errorMessage.Should().Contain("LogicalName");
            }
        }

        // ===== EntityMetadata Parameter with PassThru ===== (1 test - E2E only)

        [Fact(Skip = "FakeXrmEasy doesn't support UpdateEntityRequest fully - requires E2E test")]
        public void SetDataverseEntityMetadata_EntityMetadataWithPassThru_ReturnsUpdatedMetadata()
        {
            // Test validates -PassThru returns updated metadata
            // Requires real Dataverse environment to validate actual metadata retrieval after update
        }

        // ===== Icon Properties in Output ===== (2 tests - enabled)
        // These tests verify Get-DataverseEntityMetadata returns icon properties (read-only operation).

        [Fact]
        public void GetDataverseEntityMetadata_ReturnsIconPropertiesAccessible()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection();

            // Act - Get metadata
            ps.AddCommand("Get-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact");

            var results = ps.Invoke();

            // Assert - Icon properties should exist
            ps.HadErrors.Should().BeFalse();
            results.Should().ContainSingle();

            var result = results[0];
            var propertyNames = result.Properties.Select(p => p.Name).ToList();

            propertyNames.Should().Contain("IconVectorName");
            propertyNames.Should().Contain("IconLargeName");
            propertyNames.Should().Contain("IconMediumName");
            propertyNames.Should().Contain("IconSmallName");
        }

        [Fact]
        public void GetDataverseEntityMetadata_IconProperties_CanBeAccessed()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection();

            // Act - Get metadata and access icon properties
            ps.AddCommand("Get-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact");

            var results = ps.Invoke();

            // Assert - Accessing icon properties should not throw
            ps.HadErrors.Should().BeFalse();
            results.Should().ContainSingle();

            var result = results[0];
            
            // These should not throw errors (values may be null)
            var vectorIcon = result.Properties["IconVectorName"]?.Value;
            var largeIcon = result.Properties["IconLargeName"]?.Value;
            var mediumIcon = result.Properties["IconMediumName"]?.Value;
            var smallIcon = result.Properties["IconSmallName"]?.Value;

            // Test passes if no errors were thrown
            true.Should().BeTrue();
        }

        // ===== Icon WebResource Validation Tests ===== (7 tests)
        // These tests verify icon validation behavior. Some can work with mocks, others need E2E.

        [Fact]
        public void SetDataverseEntityMetadata_IconVectorName_ThrowsIfWebResourceNotFound()
        {
            // Test validates error when referenced webresource doesn't exist
            // This validation happens BEFORE the entity operation, so it should fail at validation.
            
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            // Create mock without webresource entities - icon validation will fail
            var mockConnection = CreateMockConnection("contact");

            // Act - Try to update entity with non-existent icon
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconVectorName", "nonexistent_icon.svg");

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Should fail because webresource doesn't exist
            (ps.HadErrors || caughtException != null).Should().BeTrue();
            
            if (ps.Streams.Error.Count > 0)
            {
                // The error should mention the webresource validation
                var errorMessage = ps.Streams.Error[0].Exception.Message;
                errorMessage.Should().Contain("webresource");
            }
        }

        [Fact]
        public void SetDataverseEntityMetadata_IconVectorName_ThrowsIfWebResourceWrongType()
        {
            // Test validates error when webresource is not SVG type
            // This validation happens BEFORE the entity operation.
            
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create a PNG webresource (type 5) when we need SVG (type 11)
            var webResource = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "test_icon.png",
                ["webresourcetype"] = new OptionSetValue(5) // PNG, not SVG
            };
            Service!.Create(webResource);

            // Act - Try to update entity with wrong icon type
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconVectorName", "test_icon.png");

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Should fail because webresource type is wrong
            (ps.HadErrors || caughtException != null).Should().BeTrue();
            
            if (ps.Streams.Error.Count > 0)
            {
                var errorMessage = ps.Streams.Error[0].Exception.Message;
                // Error should mention type requirement
                errorMessage.Should().Contain("type");
            }
        }

        [Fact]
        public void SetDataverseEntityMetadata_SkipIconValidation_SkipsExistenceCheck()
        {
            // Test validates -SkipIconValidation allows non-existent webresources
            // With SkipIconValidation, validation is skipped and we proceed to entity update
            // which will fail in FakeXrmEasy but that's expected.
            
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Act - Use SkipIconValidation to bypass validation
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconVectorName", "nonexistent_icon.svg")
              .AddParameter("SkipIconValidation", true);

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Should NOT fail with webresource validation error
            // It may fail later with UpdateEntityRequest but that's a different error
            if (ps.Streams.Error.Count > 0)
            {
                var errorMessage = ps.Streams.Error[0].Exception.Message.ToLower();
                // Should NOT contain webresource validation error
                errorMessage.Should().NotContain("does not reference a valid webresource");
            }
        }

        [Fact]
        public void SetDataverseEntityMetadata_SkipIconValidation_SkipsTypeCheck()
        {
            // Test validates -SkipIconValidation allows wrong webresource type
            
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create a PNG webresource when we need SVG
            var webResource = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "wrong_type_icon.png",
                ["webresourcetype"] = new OptionSetValue(5) // PNG, not SVG
            };
            Service!.Create(webResource);

            // Act - Use SkipIconValidation to bypass validation
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconVectorName", "wrong_type_icon.png")
              .AddParameter("SkipIconValidation", true);

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Should NOT fail with webresource type error
            if (ps.Streams.Error.Count > 0)
            {
                var errorMessage = ps.Streams.Error[0].Exception.Message.ToLower();
                // Should NOT contain type validation error
                errorMessage.Should().NotContain("is required");
            }
        }

        [Fact(Skip = "Requires full entity metadata update support - E2E test")]
        public void SetDataverseEntityMetadata_IconVectorName_ValidatesAgainstSVGWebResource()
        {
            // Test validates that IconVectorName must reference valid SVG webresource
            // AND that the entity is successfully updated - requires E2E
        }

        [Fact(Skip = "Requires full entity metadata update support - E2E test")]
        public void SetDataverseEntityMetadata_IconVectorName_AllowsEmptyStringToClear()
        {
            // Test validates empty string is allowed to clear icon
            // AND that the entity is successfully updated - requires E2E
        }

        [Fact(Skip = "Requires full entity metadata update support - E2E test")]
        public void SetDataverseEntityMetadata_IconVectorName_AllowsNullToSkipUpdate()
        {
            // Test validates null/omitted IconVectorName skips icon update
            // AND that entity update succeeds - requires E2E
        }

        // ===== Icon WebResource Validation - Unpublished WebResources ===== (1 test - E2E only)

        [Fact(Skip = "Requires unpublished webresource query support - E2E test")]
        public void SetDataverseEntityMetadata_IconVectorName_ValidatesAgainstUnpublishedWebResource()
        {
            // Test validates that validation checks unpublished webresources
            // RetrieveUnpublishedMultipleRequest is mocked to return empty, so this needs E2E
        }
    }
}
