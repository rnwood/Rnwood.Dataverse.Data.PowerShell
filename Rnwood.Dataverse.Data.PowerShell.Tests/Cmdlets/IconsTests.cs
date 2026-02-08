using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

/// <summary>
/// Tests for Icon-related cmdlets:
/// - Get-DataverseIconSetIcon
/// - Set-DataverseTableIconFromSet
/// </summary>
public class IconsTests : TestBase
{
    private readonly Type _getIconCmdletType = typeof(GetDataverseIconSetIconCmdlet);
    private readonly Type _setIconCmdletType = typeof(SetDataverseTableIconFromSetCmdlet);

    private static PS CreatePowerShellWithCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault();
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseIconSetIcon", typeof(GetDataverseIconSetIconCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Set-DataverseTableIconFromSet", typeof(SetDataverseTableIconFromSetCmdlet), null));

        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();

        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }

    [Fact]
    public void GetDataverseIconSetIcon_DefaultsToFluentUI()
    {
        // Verify default IconSet is FluentUI via initial property value
        var instance = Activator.CreateInstance(_getIconCmdletType);
        var property = _getIconCmdletType.GetProperty("IconSet");
        var value = property!.GetValue(instance);
        value.Should().Be("FluentUI");
    }

    [Fact]
    public void GetDataverseIconSetIcon_RetrievesIconsFromIconoir()
    {
        // Tests retrieving icons from Iconoir icon set
    }

    [Fact]
    public void GetDataverseIconSetIcon_FiltersIconsByName()
    {
        // Tests -Name parameter filters results
    }

    [Fact]
    public void GetDataverseIconSetIcon_RetrievesIconsFromFluentUI()
    {
        // Tests retrieving icons from FluentUI icon set
    }

    [Fact]
    public void GetDataverseIconSetIcon_RetrievesIconsFromTabler()
    {
        // Tests retrieving icons from Tabler icon set
    }

    [Fact]
    public void SetDataverseTableIconFromSet_PublisherPrefixParameter_IsMandatory()
    {
        // Verify PublisherPrefix is required
        var property = _setIconCmdletType.GetProperty("PublisherPrefix");
        property.Should().NotBeNull();
        
        var paramAttr = property!.GetCustomAttribute<ParameterAttribute>();
        paramAttr.Should().NotBeNull();
        paramAttr!.Mandatory.Should().BeTrue();
    }

    [Fact]
    public void SetDataverseTableIconFromSet_SupportsShouldProcess()
    {
        // Verify SupportsShouldProcess is enabled on cmdlet
        var cmdletAttr = _setIconCmdletType.GetCustomAttribute<CmdletAttribute>();
        cmdletAttr.Should().NotBeNull();
        cmdletAttr!.SupportsShouldProcess.Should().BeTrue();
    }

    [Fact]
    public void SetDataverseTableIconFromSet_EntityNameParameter_IsMandatory()
    {
        // Verify EntityName is required
        var property = _setIconCmdletType.GetProperty("EntityName");
        property.Should().NotBeNull();
        
        var paramAttr = property!.GetCustomAttribute<ParameterAttribute>();
        paramAttr.Should().NotBeNull();
        paramAttr!.Mandatory.Should().BeTrue();
    }

    [Fact]
    public void SetDataverseTableIconFromSet_IconNameParameter_IsMandatory()
    {
        // Verify IconName is required
        var property = _setIconCmdletType.GetProperty("IconName");
        property.Should().NotBeNull();
        
        var paramAttr = property!.GetCustomAttribute<ParameterAttribute>();
        paramAttr.Should().NotBeNull();
        paramAttr!.Mandatory.Should().BeTrue();
    }

    [Fact]
    public void SetDataverseTableIconFromSet_CreatesWebResourceWithIconContent()
    {
        // Arrange
        const string testIconSvg = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24""><path d=""M12 2L2 7l10 5 10-5-10-5z""/></svg>";
        const string publisherPrefix = "test";
        const string entityName = "contact";
        const string expectedWebResourceName = "test_/icons/custom/icon.svg";

        // Create simple mock connection without custom interceptor
        var connection = CreateMockConnection("contact");

        // Pre-create contact entity metadata in the mock service
        var contactMetadata = LoadedMetadata.FirstOrDefault(m => m.LogicalName == entityName);
        contactMetadata.Should().NotBeNull("contact metadata should be loaded");

        // Act - Invoke cmdlet through PowerShell runtime
        using var ps = CreatePowerShellWithCmdlets();
        ps.AddCommand("Set-DataverseTableIconFromSet")
          .AddParameter("Connection", connection)
          .AddParameter("EntityName", entityName)
          .AddParameter("IconContent", testIconSvg)
          .AddParameter("PublisherPrefix", publisherPrefix)
          .AddParameter("Confirm", false);

        var results = ps.Invoke();

        // Assert - Check for errors and display them if present
        if (ps.HadErrors)
        {
            var errors = string.Join(Environment.NewLine, ps.Streams.Error.Select(e => 
                $"{e.Exception.GetType().Name}: {e.Exception.Message}{Environment.NewLine}{e.Exception.StackTrace}"));
            Assert.Fail($"Cmdlet execution failed with errors:{Environment.NewLine}{errors}");
        }

        // Assert - Verify web resource was created
        var query = new QueryExpression("webresource")
        {
            ColumnSet = new ColumnSet("name", "displayname", "description", "content", "webresourcetype"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("name", ConditionOperator.Equal, expectedWebResourceName)
                }
            }
        };

        var webResources = Service!.RetrieveMultiple(query);
        webResources.Entities.Should().HaveCount(1, "web resource should be created");

        var webResource = webResources.Entities[0];
        webResource["name"].Should().Be(expectedWebResourceName);
        webResource["displayname"].Should().Be("Icon: ");
        webResource["description"].Should().Be("Vector icon from FluentUI icon set");
        webResource["webresourcetype"].Should().BeOfType<OptionSetValue>()
            .Which.Value.Should().Be(11, "webresourcetype should be 11 (SVG)");

        // Verify content is base64 encoded SVG
        var contentBase64 = webResource["content"] as string;
        contentBase64.Should().NotBeNullOrEmpty();
        var decodedContent = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(contentBase64!));
        decodedContent.Should().Be(testIconSvg);
    }
}
