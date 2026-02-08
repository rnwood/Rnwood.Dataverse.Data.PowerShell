using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using FluentAssertions;
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

    // Get-DataverseIconSetIcon Tests (IconSetCmdlets.Tests.ps1 - ~10 tests)

    [Fact]
    public void GetDataverseIconSetIcon_CmdletExists()
    {
        // Verify cmdlet has proper CmdletAttribute
        var cmdletAttr = _getIconCmdletType.GetCustomAttribute<CmdletAttribute>();
        cmdletAttr.Should().NotBeNull();
        cmdletAttr!.VerbName.Should().Be("Get");
        cmdletAttr.NounName.Should().Be("DataverseIconSetIcon");
    }

    [Fact]
    public void GetDataverseIconSetIcon_AcceptsIconSetParameter()
    {
        // Verify IconSet parameter exists
        var property = _getIconCmdletType.GetProperty("IconSet");
        property.Should().NotBeNull();
        
        var paramAttr = property!.GetCustomAttribute<ParameterAttribute>();
        paramAttr.Should().NotBeNull();
        paramAttr!.Position.Should().Be(0);
    }

    [Fact]
    public void GetDataverseIconSetIcon_AcceptsNameParameterWithWildcards()
    {
        // Verify Name parameter exists and supports wildcards
        var property = _getIconCmdletType.GetProperty("Name");
        property.Should().NotBeNull();
        
        var paramAttr = property!.GetCustomAttribute<ParameterAttribute>();
        paramAttr.Should().NotBeNull();
        
        var wildcardsAttr = property.GetCustomAttribute<SupportsWildcardsAttribute>();
        wildcardsAttr.Should().NotBeNull();
    }

    [Fact]
    public void GetDataverseIconSetIcon_IconSetParameter_HasValidateSetAttribute()
    {
        // Verify IconSet has ValidateSet (FluentUI, Iconoir, Tabler, etc.)
        var property = _getIconCmdletType.GetProperty("IconSet");
        property.Should().NotBeNull();
        
        var validateSetAttr = property!.GetCustomAttribute<ValidateSetAttribute>();
        validateSetAttr.Should().NotBeNull();
        validateSetAttr!.ValidValues.Should().Contain("FluentUI");
        validateSetAttr.ValidValues.Should().Contain("Iconoir");
        validateSetAttr.ValidValues.Should().Contain("Tabler");
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

    [Fact(Skip = "Requires internet access - downloads icons from CDN")]
    public void GetDataverseIconSetIcon_RetrievesIconsFromIconoir()
    {
        // Tests retrieving icons from Iconoir icon set
    }

    [Fact(Skip = "Requires internet access - downloads icons from CDN")]
    public void GetDataverseIconSetIcon_FiltersIconsByName()
    {
        // Tests -Name parameter filters results
    }

    [Fact(Skip = "Requires internet access - downloads icons from CDN")]
    public void GetDataverseIconSetIcon_RetrievesIconsFromFluentUI()
    {
        // Tests retrieving icons from FluentUI icon set
    }

    [Fact(Skip = "Requires internet access - downloads icons from CDN")]
    public void GetDataverseIconSetIcon_RetrievesIconsFromTabler()
    {
        // Tests retrieving icons from Tabler icon set
    }

    // Set-DataverseTableIconFromSet Tests (~12+ tests)

    [Fact]
    public void SetDataverseTableIconFromSet_CmdletExists()
    {
        // Verify cmdlet has proper CmdletAttribute
        var cmdletAttr = _setIconCmdletType.GetCustomAttribute<CmdletAttribute>();
        cmdletAttr.Should().NotBeNull();
        cmdletAttr!.VerbName.Should().Be("Set");
        cmdletAttr.NounName.Should().Be("DataverseTableIconFromSet");
    }

    [Fact]
    public void SetDataverseTableIconFromSet_AcceptsEntityNameParameter()
    {
        // Verify EntityName parameter exists
        var property = _setIconCmdletType.GetProperty("EntityName");
        property.Should().NotBeNull();
        
        var paramAttr = property!.GetCustomAttribute<ParameterAttribute>();
        paramAttr.Should().NotBeNull();
    }

    [Fact]
    public void SetDataverseTableIconFromSet_AcceptsIconSetParameter()
    {
        // Verify IconSet parameter exists
        var property = _setIconCmdletType.GetProperty("IconSet");
        property.Should().NotBeNull();
        
        var paramAttr = property!.GetCustomAttribute<ParameterAttribute>();
        paramAttr.Should().NotBeNull();
    }

    [Fact]
    public void SetDataverseTableIconFromSet_AcceptsIconNameParameter()
    {
        // Verify IconName parameter exists
        var property = _setIconCmdletType.GetProperty("IconName");
        property.Should().NotBeNull();
        
        var paramAttr = property!.GetCustomAttribute<ParameterAttribute>();
        paramAttr.Should().NotBeNull();
    }

    [Fact]
    public void SetDataverseTableIconFromSet_AcceptsPublisherPrefixParameter()
    {
        // Verify PublisherPrefix parameter exists
        var property = _setIconCmdletType.GetProperty("PublisherPrefix");
        property.Should().NotBeNull();
        
        var paramAttr = property!.GetCustomAttribute<ParameterAttribute>();
        paramAttr.Should().NotBeNull();
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
    public void SetDataverseTableIconFromSet_AcceptsPublishSwitch()
    {
        // Verify Publish switch exists
        var property = _setIconCmdletType.GetProperty("Publish");
        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(SwitchParameter));
    }

    [Fact]
    public void SetDataverseTableIconFromSet_AcceptsPassThruSwitch()
    {
        // Verify PassThru switch exists
        var property = _setIconCmdletType.GetProperty("PassThru");
        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(SwitchParameter));
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

    [Fact(Skip = "Requires E2E testing - downloads icon and creates web resources")]
    public void SetDataverseTableIconFromSet_DownloadsIconAndCreatesWebResources()
    {
        // Tests end-to-end:
        // 1. Downloads SVG icon from CDN
        // 2. Creates web resources for vector and raster icons
        // 3. Updates entity metadata with icon references
        // 4. Optionally publishes metadata
    }
}
