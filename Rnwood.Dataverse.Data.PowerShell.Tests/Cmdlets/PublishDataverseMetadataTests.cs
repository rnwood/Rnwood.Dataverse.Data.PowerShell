using FluentAssertions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for metadata cmdlets - Publish parameter and behavior
    /// Migrated from tests/Set-DataverseMetadata-Publish.Tests.ps1 (11 tests)
    /// </summary>
    public class PublishDataverseMetadataTests : TestBase
    {
        // ===== Set-DataverseEntityMetadata -Publish Parameter ===== (2 tests)

        [Fact]
        public void SetDataverseEntityMetadata_HasPublishParameter()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityMetadata", typeof(Commands.SetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Act - Get command info
            ps.AddCommand("Get-Command")
              .AddParameter("Name", "Set-DataverseEntityMetadata");

            var results = ps.Invoke();

            // Assert - Should have Publish parameter
            results.Should().ContainSingle();
            var commandInfo = results[0].BaseObject as CommandInfo;
            commandInfo.Should().NotBeNull();

            var param = commandInfo!.Parameters["Publish"];
            param.Should().NotBeNull();
            param.ParameterType.Name.Should().Be("SwitchParameter");
        }

        [Fact]
        public void SetDataverseEntityMetadata_WhatIfWithPublish_DoesNotPublish()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityMetadata", typeof(Commands.SetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - use -WhatIf with -Publish
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("DisplayName", "Updated Contact")
              .AddParameter("Publish", true)
              .AddParameter("WhatIf", true);

            var results = ps.Invoke();

            // Assert - WhatIf should prevent both update and publish
            // WhatIf returns nothing but shouldn't error
            results.Should().BeEmpty();
            ps.HadErrors.Should().BeFalse();
        }

        // ===== Set-DataverseAttributeMetadata -Publish Parameter ===== (2 tests)

        [Fact]
        public void SetDataverseAttributeMetadata_HasPublishParameter()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Act - Get command info
            ps.AddCommand("Get-Command")
              .AddParameter("Name", "Set-DataverseAttributeMetadata");

            var results = ps.Invoke();

            // Assert - Should have Publish parameter
            results.Should().ContainSingle();
            var commandInfo = results[0].BaseObject as CommandInfo;
            commandInfo.Should().NotBeNull();

            var param = commandInfo!.Parameters["Publish"];
            param.Should().NotBeNull();
            param.ParameterType.Name.Should().Be("SwitchParameter");
        }

        [Fact]
        public void SetDataverseAttributeMetadata_WhatIfWithPublish_DoesNotPublish()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - use -WhatIf with -Publish on existing attribute
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "firstname")
              .AddParameter("DisplayName", "Updated First Name")
              .AddParameter("Publish", true)
              .AddParameter("WhatIf", true);

            var results = ps.Invoke();

            // Assert - WhatIf should prevent both update and publish
            results.Should().BeEmpty();
            ps.HadErrors.Should().BeFalse();
        }

        // ===== Set-DataverseRelationshipMetadata -Publish Parameter ===== (2 tests)

        [Fact]
        public void SetDataverseRelationshipMetadata_HasPublishParameter()
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

            // Assert - Should have Publish parameter
            results.Should().ContainSingle();
            var commandInfo = results[0].BaseObject as CommandInfo;
            commandInfo.Should().NotBeNull();

            var param = commandInfo!.Parameters["Publish"];
            param.Should().NotBeNull();
            param.ParameterType.Name.Should().Be("SwitchParameter");
        }

        [Fact(Skip = "FakeXrmEasy doesn't support RetrieveRelationshipRequest - requires E2E test")]
        public void SetDataverseRelationshipMetadata_WhatIfWithPublish_DoesNotPublish()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseRelationshipMetadata", typeof(Commands.SetDataverseRelationshipMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact", "account");

            // Act - use -WhatIf with -Publish (requires all mandatory parameters)
            ps.AddCommand("Set-DataverseRelationshipMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("SchemaName", "contact_customer_accounts")
              .AddParameter("RelationshipType", "OneToMany")
              .AddParameter("ReferencedEntity", "account")
              .AddParameter("ReferencingEntity", "contact")
              .AddParameter("Publish", true)
              .AddParameter("WhatIf", true);

            var results = ps.Invoke();

            // Assert - WhatIf should prevent both update and publish
            results.Should().BeEmpty();
            ps.HadErrors.Should().BeFalse();
        }

        // ===== RetrieveAsIfPublished Behavior ===== (2 tests - skipped)

        [Fact(Skip = "FakeXrmEasy doesn't support RetrieveAsIfPublished distinction - requires E2E test")]
        public void EntityMetadataRetrieval_RetrievesUnpublishedChanges()
        {
            // Test validates that RetrieveAsIfPublished=true retrieves unpublished entity changes
            // This prevents errors when updating entities that have unpublished changes
            // Requires real Dataverse environment to validate
        }

        [Fact(Skip = "FakeXrmEasy doesn't support RetrieveAsIfPublished distinction - requires E2E test")]
        public void AttributeMetadataRetrieval_RetrievesUnpublishedChanges()
        {
            // Test validates that RetrieveAsIfPublished=true retrieves unpublished attribute changes
            // This prevents errors when updating attributes that have unpublished changes
            // Requires real Dataverse environment to validate
        }

        // ===== Publishing After Metadata Changes ===== (3 tests)

        [Fact]
        public void SetDataverseEntityMetadata_Publish_PublishesEntityAfterUpdate()
        {
            // Arrange - Track if PublishXmlRequest was executed
            var publishRequestExecuted = false;
            string? publishParameterXml = null;

            var mockConnection = CreateMockConnection(request =>
            {
                if (request.GetType().Name == "PublishXmlRequest")
                {
                    publishRequestExecuted = true;
                    publishParameterXml = request.Parameters["ParameterXml"]?.ToString();
                    return new PublishXmlResponse();
                }
                return null;
            }, "contact");

            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityMetadata", typeof(Commands.SetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Act - Update entity with -Publish switch
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("DisplayName", "Updated Contact")
              .AddParameter("Publish", true)
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - PublishXmlRequest should have been executed with entity name
            publishRequestExecuted.Should().BeTrue("because -Publish switch was specified");
            publishParameterXml.Should().Contain("contact", "because the entity name should be in the publish XML");
        }

        [Fact]
        public void SetDataverseEntityMetadata_NoPublish_DoesNotPublish()
        {
            // Arrange - Track if PublishXmlRequest was executed
            var publishRequestExecuted = false;

            var mockConnection = CreateMockConnection(request =>
            {
                if (request.GetType().Name == "PublishXmlRequest")
                {
                    publishRequestExecuted = true;
                    return new PublishXmlResponse();
                }
                return null;
            }, "contact");

            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityMetadata", typeof(Commands.SetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Act - Update entity WITHOUT -Publish switch
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("DisplayName", "Updated Contact")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - PublishXmlRequest should NOT have been executed
            publishRequestExecuted.Should().BeFalse("because -Publish switch was not specified");
        }

        [Fact]
        public void SetDataverseAttributeMetadata_Publish_PublishesEntityAfterAttributeUpdate()
        {
            // Arrange - Track if PublishXmlRequest was executed
            var publishRequestExecuted = false;
            string? publishParameterXml = null;

            var mockConnection = CreateMockConnection(request =>
            {
                if (request.GetType().Name == "PublishXmlRequest")
                {
                    publishRequestExecuted = true;
                    publishParameterXml = request.Parameters["ParameterXml"]?.ToString();
                    return new PublishXmlResponse();
                }
                // CreateAttributeRequest needs a response with AttributeId
                if (request.GetType().Name == "CreateAttributeRequest")
                {
                    var response = new CreateAttributeResponse();
                    response.Results["AttributeId"] = Guid.NewGuid();
                    return response;
                }
                // UpdateAttributeRequest needs an empty response
                if (request.GetType().Name == "UpdateAttributeRequest")
                {
                    return new UpdateAttributeResponse();
                }
                return null;
            }, "contact");

            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Act - Update existing attribute (firstname exists in contact) with -Publish switch
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "firstname")
              .AddParameter("DisplayName", "Updated First Name")
              .AddParameter("Publish", true)
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - PublishXmlRequest should have been executed with entity name
            publishRequestExecuted.Should().BeTrue("because -Publish switch was specified");
            publishParameterXml.Should().Contain("contact", "because the entity name should be in the publish XML");
        }

        [Fact(Skip = "FakeXrmEasy doesn't support RetrieveRelationshipRequest - requires E2E test")]
        public void SetDataverseRelationshipMetadata_Publish_PublishesBothEntitiesAfterRelationshipUpdate()
        {
            // This test requires RetrieveRelationshipRequest support which FakeXrmEasy doesn't provide.
            // The cmdlet needs to retrieve existing relationship metadata before updating.
            // Test this functionality in e2e-tests/RelationshipMetadata.Tests.ps1 instead.
        }
    }
}
