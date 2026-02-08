using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using FluentAssertions;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Discovery;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class SetDataverseConnectionAsDefaultTests : TestBase
{
    private readonly Type _cmdletType = typeof(SetDataverseConnectionAsDefaultCmdlet);

    private static PS CreatePowerShellWithCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault();
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Set-DataverseConnectionAsDefault", typeof(SetDataverseConnectionAsDefaultCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseConnection", typeof(GetDataverseConnectionCmdlet), null));

        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();

        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }

    // ===== Parameter Metadata Tests =====

    [Fact]
    public void SetDataverseConnectionAsDefault_CmdletExists()
    {
        // Verify cmdlet has proper CmdletAttribute
        var cmdletAttr = _cmdletType.GetCustomAttribute<CmdletAttribute>();
        cmdletAttr.Should().NotBeNull();
        cmdletAttr!.VerbName.Should().Be("Set");
        cmdletAttr.NounName.Should().Be("DataverseConnectionAsDefault");
    }

    [Fact]
    public void SetDataverseConnectionAsDefault_ConnectionParameter_Exists()
    {
        // Verify Connection parameter exists
        var property = _cmdletType.GetProperty("Connection");
        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(ServiceClient));
    }

    [Fact]
    public void SetDataverseConnectionAsDefault_ConnectionParameter_IsMandatory()
    {
        // Verify Connection parameter is mandatory
        var property = _cmdletType.GetProperty("Connection");
        var paramAttr = property!.GetCustomAttribute<ParameterAttribute>();
        paramAttr.Should().NotBeNull();
        paramAttr!.Mandatory.Should().BeTrue();
    }

    [Fact]
    public void SetDataverseConnectionAsDefault_ConnectionParameter_AcceptsFromPipeline()
    {
        // Verify Connection parameter accepts pipeline input
        var property = _cmdletType.GetProperty("Connection");
        var paramAttr = property!.GetCustomAttribute<ParameterAttribute>();
        paramAttr.Should().NotBeNull();
        paramAttr!.ValueFromPipeline.Should().BeTrue();
    }

    // ===== Execution Tests =====

    [Fact]
    public void SetDataverseConnectionAsDefault_ExecutesWithoutErrors()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");

        // Act - Set connection as default
        ps.AddCommand("Set-DataverseConnectionAsDefault")
          .AddParameter("Connection", mockConnection);
        ps.Invoke();

        // Assert - Command executed without errors  
        ps.HadErrors.Should().BeFalse();
    }

    [Fact]
    public void SetDataverseConnectionAsDefault_AcceptsPipelineInput()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");

        // Act - Set connection as default via pipeline
        ps.AddCommand("Set-DataverseConnectionAsDefault");
        ps.Invoke(new[] { mockConnection });

        // Assert - Verify no errors
        ps.HadErrors.Should().BeFalse();
    }
}
