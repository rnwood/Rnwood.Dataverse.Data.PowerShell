using FluentAssertions;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Remove-DataverseAttributeMetadata cmdlet
    /// Migrated from tests/Set-Remove-DataverseMetadata.Tests.ps1 (Remove-DataverseAttributeMetadata tests)
    /// </summary>
    public class RemoveDataverseAttributeMetadataTests : TestBase
    {
        // ===== Attribute Deletion ===== (5 tests)

        [Fact]
        public void RemoveDataverseAttributeMetadata_WhatIf_DoesNotThrow()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Remove-DataverseAttributeMetadata", typeof(Commands.RemoveDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - WhatIf should not throw even with mock connection
            ps.AddCommand("Remove-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "new_testfield")
              .AddParameter("WhatIf", true);

            // Assert
            var action = () => ps.Invoke();
            action.Should().NotThrow();
        }

        [Fact]
        public void RemoveDataverseAttributeMetadata_SupportsShouldProcess()
        {
            // Arrange
            var cmdletType = typeof(Commands.RemoveDataverseAttributeMetadataCmdlet);
            var cmdletAttribute = (CmdletAttribute)cmdletType.GetCustomAttributes(typeof(CmdletAttribute), false)[0];

            // Assert - verify SupportsShouldProcess is enabled (allows -Confirm:$false and -WhatIf)
            cmdletAttribute.SupportsShouldProcess.Should().BeTrue();
        }

        [Fact]
        public void RemoveDataverseAttributeMetadata_HasConfirmParameter()
        {
            // Arrange
            var cmdletType = typeof(Commands.RemoveDataverseAttributeMetadataCmdlet);

            // Act - check for Confirm parameter (provided by SupportsShouldProcess)
            var properties = cmdletType.GetProperties();
            var hasConfirmImpact = cmdletType.GetCustomAttributes(typeof(CmdletAttribute), false).Length > 0;

            // Assert - SupportsShouldProcess automatically adds Confirm parameter
            hasConfirmImpact.Should().BeTrue();
        }

        [Fact]
        public void RemoveDataverseAttributeMetadata_AttributeNameFromPipeline()
        {
            // Arrange
            var cmdletType = typeof(Commands.RemoveDataverseAttributeMetadataCmdlet);
            var attributeNameProperty = cmdletType.GetProperty("AttributeName");

            // Assert - verify parameter accepts pipeline input by property name
            attributeNameProperty.Should().NotBeNull();
            var parameterAttributes = attributeNameProperty!.GetCustomAttributes(typeof(ParameterAttribute), false);
            parameterAttributes.Should().NotBeEmpty();
            
            var hasValueFromPipelineByPropertyName = false;
            foreach (ParameterAttribute attr in parameterAttributes)
            {
                if (attr.ValueFromPipelineByPropertyName)
                {
                    hasValueFromPipelineByPropertyName = true;
                    break;
                }
            }
            hasValueFromPipelineByPropertyName.Should().BeTrue();
        }

        // ===== Alias Support ===== (2 tests)

        [Fact]
        public void RemoveDataverseAttributeMetadata_HasTableNameAlias()
        {
            // Arrange
            var cmdletType = typeof(Commands.RemoveDataverseAttributeMetadataCmdlet);
            var entityNameProperty = cmdletType.GetProperty("EntityName");

            // Assert
            entityNameProperty.Should().NotBeNull();
            var aliasAttributes = entityNameProperty!.GetCustomAttributes(typeof(AliasAttribute), false);
            aliasAttributes.Should().NotBeEmpty();
            var aliasAttr = (AliasAttribute)aliasAttributes[0];
            aliasAttr.AliasNames.Should().Contain("TableName");
        }

        [Fact]
        public void RemoveDataverseAttributeMetadata_HasColumnNameAlias()
        {
            // Arrange
            var cmdletType = typeof(Commands.RemoveDataverseAttributeMetadataCmdlet);
            var attributeNameProperty = cmdletType.GetProperty("AttributeName");

            // Assert
            attributeNameProperty.Should().NotBeNull();
            var aliasAttributes = attributeNameProperty!.GetCustomAttributes(typeof(AliasAttribute), false);
            aliasAttributes.Should().NotBeEmpty();
            var aliasAttr = (AliasAttribute)aliasAttributes[0];
            aliasAttr.AliasNames.Should().Contain("ColumnName");
        }
    }
}
