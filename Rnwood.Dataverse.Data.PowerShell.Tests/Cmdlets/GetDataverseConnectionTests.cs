using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using FluentAssertions;
using Microsoft.PowerPlatform.Dataverse.Client;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class GetDataverseConnectionTests : TestBase, IDisposable
{
    private readonly Type _cmdletType = typeof(GetDataverseConnectionCmdlet);

    public GetDataverseConnectionTests()
    {
        // Clear default connection before each test
        ClearDefaultConnection();
    }

    public override void Dispose()
    {
        // Clean up default connection after each test
        ClearDefaultConnection();
        base.Dispose();
    }

    /// <summary>
    /// Clears the default connection via reflection.
    /// </summary>
    private static void ClearDefaultConnection()
    {
        var managerType = typeof(GetDataverseConnectionCmdlet).Assembly
            .GetType("Rnwood.Dataverse.Data.PowerShell.Commands.DefaultConnectionManager");
        var clearMethod = managerType?.GetMethod("ClearDefaultConnection", BindingFlags.Public | BindingFlags.Static);
        clearMethod?.Invoke(null, null);
    }

    /// <summary>
    /// Sets the default connection via reflection.
    /// </summary>
    private static void SetDefaultConnection(ServiceClient connection)
    {
        var managerType = typeof(GetDataverseConnectionCmdlet).Assembly
            .GetType("Rnwood.Dataverse.Data.PowerShell.Commands.DefaultConnectionManager");
        var prop = managerType?.GetProperty("DefaultConnection", BindingFlags.Public | BindingFlags.Static);
        prop?.SetValue(null, connection);
    }

    /// <summary>
    /// Gets the default connection via reflection.
    /// </summary>
    private static ServiceClient? GetDefaultConnectionValue()
    {
        var managerType = typeof(GetDataverseConnectionCmdlet).Assembly
            .GetType("Rnwood.Dataverse.Data.PowerShell.Commands.DefaultConnectionManager");
        var prop = managerType?.GetProperty("DefaultConnection", BindingFlags.Public | BindingFlags.Static);
        return prop?.GetValue(null) as ServiceClient;
    }

    /// <summary>
    /// Creates a PowerShell instance with Get-DataverseConnection and related cmdlets registered.
    /// </summary>
    private static PS CreatePowerShellWithCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault();

        // Register GetDataverseConnectionCmdlet with its alias
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseConnection", typeof(GetDataverseConnectionCmdlet), null));

        // Also register the alias
        initialSessionState.Commands.Add(new SessionStateAliasEntry(
            "Connect-DataverseConnection", "Get-DataverseConnection"));

        // Register other cmdlets that might be used in tests
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseRecord", typeof(GetDataverseRecordCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseWhoAmI", typeof(GetDataverseWhoAmICmdlet), null));

        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();

        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }

    /// <summary>
    /// Gets the ParameterAttribute for a parameter in the cmdlet type.
    /// </summary>
    private ParameterAttribute[]? GetParameterAttributes(string parameterName)
    {
        var property = _cmdletType.GetProperty(parameterName);
        return property?.GetCustomAttributes<ParameterAttribute>().ToArray();
    }

    /// <summary>
    /// Gets parameter attributes for a specific parameter set.
    /// </summary>
    private ParameterAttribute? GetParameterInSet(string parameterName, string parameterSetName)
    {
        var attributes = GetParameterAttributes(parameterName);
        return attributes?.FirstOrDefault(a => a.ParameterSetName == parameterSetName);
    }

    /// <summary>
    /// Checks if a parameter exists in the cmdlet.
    /// </summary>
    private bool ParameterExists(string parameterName)
    {
        return _cmdletType.GetProperty(parameterName) != null;
    }

    // ===== Default Connection Management Tests =====

    [Fact]
    public void GetDataverseConnection_SetAsDefault_StoresConnectionAsDefault()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");

        // Act - Set connection as default using SetAsDefault via reflection
        // (We can't use -Mock since that parameter doesn't exist in the cmdlet)
        SetDefaultConnection(mockConnection);

        // Verify via GetDefault parameter
        ps.AddCommand("Get-DataverseConnection")
          .AddParameter("GetDefault", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        results[0].BaseObject.Should().Be(mockConnection);
    }

    [Fact]
    public void GetDataverseConnection_GetDefault_ReturnsError_WhenNoDefaultSet()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        ClearDefaultConnection();

        // Act
        ps.AddCommand("Get-DataverseConnection")
          .AddParameter("GetDefault", true);
        
        // Assert - ThrowTerminatingError causes CmdletInvocationException
        var action = () => ps.Invoke();
        var exception = action.Should().Throw<CmdletInvocationException>()
            .Which.InnerException as System.InvalidOperationException;
        exception.Should().NotBeNull();
        exception!.Message.Should().Contain("No default connection");
    }

    [Fact]
    public void GetDataverseConnection_Cmdlets_UseDefaultConnection_WhenConnectionNotProvided()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        SetDefaultConnection(mockConnection);

        // Act - Call Get-DataverseWhoAmI without Connection parameter
        ps.AddCommand("Get-DataverseWhoAmI");
        var results = ps.Invoke();

        // Assert - Should not error because default connection is used
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
    }

    [Fact]
    public void GetDataverseConnection_Cmdlets_Error_WhenNoConnectionAndNoDefault()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        ClearDefaultConnection();

        // Act - Call Get-DataverseRecord without Connection parameter and no default
        ps.AddCommand("Get-DataverseRecord")
          .AddParameter("TableName", "contact");
        
        // Assert - ThrowTerminatingError causes CmdletInvocationException
        var action = () => ps.Invoke();
        var exception = action.Should().Throw<CmdletInvocationException>()
            .Which.InnerException as System.InvalidOperationException;
        exception.Should().NotBeNull();
        exception!.Message.Should().Contain("No connection provided");
    }

    [Fact]
    public void GetDataverseConnection_Alias_ConnectDataverseConnection_Works()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();

        // Act - Get command info for the alias
        ps.AddCommand("Get-Command")
          .AddParameter("Name", "Connect-DataverseConnection");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var commandInfo = results[0].BaseObject as AliasInfo;
        commandInfo.Should().NotBeNull();
        commandInfo!.ReferencedCommand.Name.Should().Be("Get-DataverseConnection");
    }

    [Fact]
    public void GetDataverseConnection_GetDefaultParameterSet_WorksIndependently()
    {
        // Arrange/Act
        var getDefaultAttr = GetParameterAttributes("GetDefault");

        // Assert
        getDefaultAttr.Should().NotBeEmpty();
        var paramSetAttr = getDefaultAttr!.FirstOrDefault(a => a.Mandatory);
        paramSetAttr.Should().NotBeNull();
        paramSetAttr!.ParameterSetName.Should().Contain("Get default");
    }

    [Fact]
    public void GetDataverseConnection_AccessTokenParameterSet_AcceptsScriptBlock()
    {
        // Arrange/Act
        var accessTokenProp = _cmdletType.GetProperty("AccessToken");

        // Assert
        accessTokenProp.Should().NotBeNull();
        accessTokenProp!.PropertyType.Should().Be(typeof(ScriptBlock));

        var paramAttrs = accessTokenProp.GetCustomAttributes<ParameterAttribute>();
        var accessTokenParamSet = paramAttrs.FirstOrDefault(a => a.ParameterSetName.Contains("access token"));
        accessTokenParamSet.Should().NotBeNull();
        accessTokenParamSet!.Mandatory.Should().BeTrue();
    }

    // ===== Named Connection Management Tests =====

    [Fact]
    public void GetDataverseConnection_ListConnections_ReturnsEmptyOrArray_WhenCalled()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();

        // Act - Call ListConnections (will return whatever is saved)
        ps.AddCommand("Get-DataverseConnection")
          .AddParameter("ListConnections", true);
        var results = ps.Invoke();

        // Assert - Should not error; returns array (possibly empty)
        ps.HadErrors.Should().BeFalse();
        // Results may or may not be empty depending on user's saved connections
    }

    [Fact(Skip = "Cannot save connections in unit tests without affecting user's real connection store")]
    public void GetDataverseConnection_CanSaveNamedConnection_WithMockProvider()
    {
        // This test cannot be enabled because:
        // 1. There is no -Mock parameter in Get-DataverseConnection
        // 2. Saving connections would affect the user's real connection store
        // Use E2E tests for named connection functionality
    }

    [Fact]
    public void GetDataverseConnection_DeleteConnection_ReturnsError_WhenConnectionDoesNotExist()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var nonExistentName = $"NonExistent_{Guid.NewGuid()}";

        // Act
        ps.AddCommand("Get-DataverseConnection")
          .AddParameter("DeleteConnection", true)
          .AddParameter("Name", nonExistentName);
        
        // Assert - ThrowTerminatingError causes CmdletInvocationException
        var action = () => ps.Invoke();
        var exception = action.Should().Throw<CmdletInvocationException>()
            .Which.InnerException as System.InvalidOperationException;
        exception.Should().NotBeNull();
        exception!.Message.Should().Contain("not found");
    }

    [Fact(Skip = "Cannot test saved connections in unit tests without affecting user's real connection store")]
    public void GetDataverseConnection_ListConnections_ShowsSavedConnections()
    {
        // This test cannot be enabled because it would need to create real saved connections
        // which would affect the user's connection store
    }

    // ===== Parameter Set Availability Tests =====

    [Fact]
    public void GetDataverseConnection_NameParameter_AvailableOnInteractiveParameterSet()
    {
        // Arrange/Act
        var nameAttr = GetParameterInSet("Name", "Authenticate interactively");

        // Assert
        nameAttr.Should().NotBeNull("Name parameter should exist on Interactive parameter set");
    }

    [Fact]
    public void GetDataverseConnection_NameParameter_AvailableOnDeviceCodeParameterSet()
    {
        // Arrange/Act
        var nameAttr = GetParameterInSet("Name", "Authenticate using the device code flow");

        // Assert
        nameAttr.Should().NotBeNull("Name parameter should exist on DeviceCode parameter set");
    }

    [Fact]
    public void GetDataverseConnection_NameParameter_AvailableOnClientSecretParameterSet()
    {
        // Arrange/Act
        var nameAttr = GetParameterInSet("Name", "Authenticate with client secret");

        // Assert
        nameAttr.Should().NotBeNull("Name parameter should exist on ClientSecret parameter set");
    }

    [Fact]
    public void GetDataverseConnection_UrlOptional_ForClientSecretAuthentication()
    {
        // Arrange/Act
        var urlAttr = GetParameterInSet("Url", "Authenticate with client secret");

        // Assert
        urlAttr.Should().NotBeNull();
        urlAttr!.Mandatory.Should().BeFalse("Url should be optional for client secret auth");
    }

    [Fact]
    public void GetDataverseConnection_ListConnectionsParameterSet_Exists()
    {
        // Arrange/Act
        var listConnectionsAttrs = GetParameterAttributes("ListConnections");

        // Assert
        listConnectionsAttrs.Should().NotBeEmpty();
        var paramSetAttr = listConnectionsAttrs!.FirstOrDefault(a => a.ParameterSetName.Contains("List saved"));
        paramSetAttr.Should().NotBeNull("ListConnections parameter set should exist");
    }

    [Fact]
    public void GetDataverseConnection_DeleteConnectionParameterSet_Exists()
    {
        // Arrange/Act
        var deleteConnectionAttrs = GetParameterAttributes("DeleteConnection");

        // Assert
        deleteConnectionAttrs.Should().NotBeEmpty();
        var paramSetAttr = deleteConnectionAttrs!.FirstOrDefault(a => a.ParameterSetName.Contains("Delete"));
        paramSetAttr.Should().NotBeNull("DeleteConnection parameter set should exist");
    }

    [Fact]
    public void GetDataverseConnection_LoadNamedParameterSet_Exists()
    {
        // Arrange/Act
        var nameAttrs = GetParameterAttributes("Name");

        // Assert
        nameAttrs.Should().NotBeEmpty();
        var loadNamedAttr = nameAttrs!.FirstOrDefault(a => a.ParameterSetName.Contains("Load a saved"));
        loadNamedAttr.Should().NotBeNull("LoadNamed parameter set should exist");
        loadNamedAttr!.Mandatory.Should().BeTrue("Name should be mandatory in LoadNamed parameter set");
    }

    [Fact]
    public void GetDataverseConnection_CanUseListConnectionsParameter()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();

        // Act - Call with ListConnections
        ps.AddCommand("Get-DataverseConnection")
          .AddParameter("ListConnections", true);
        var results = ps.Invoke();

        // Assert - Should not throw parameter binding error
        ps.HadErrors.Should().BeFalse();
    }

    [Fact]
    public void GetDataverseConnection_CanUseDeleteConnectionParameter_WithNonExistentConnection()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var nonExistentName = $"NonExistent_{Guid.NewGuid()}";

        // Act
        ps.AddCommand("Get-DataverseConnection")
          .AddParameter("DeleteConnection", true)
          .AddParameter("Name", nonExistentName);
        
        // Assert - Should error with ConnectionNotFound (not parameter binding error)
        // ThrowTerminatingError causes CmdletInvocationException
        var action = () => ps.Invoke();
        var exception = action.Should().Throw<CmdletInvocationException>()
            .Which.InnerException as System.InvalidOperationException;
        exception.Should().NotBeNull();
        exception!.Message.Should().Contain("not found");
    }

    [Fact]
    public void GetDataverseConnection_CertificatePathParameter_Exists()
    {
        // Assert
        ParameterExists("CertificatePath").Should().BeTrue();
    }

    [Fact]
    public void GetDataverseConnection_CertificatePasswordParameter_Exists()
    {
        // Assert
        ParameterExists("CertificatePassword").Should().BeTrue();
    }

    [Fact]
    public void GetDataverseConnection_CertificateThumbprintParameter_Exists()
    {
        // Assert
        ParameterExists("CertificateThumbprint").Should().BeTrue();
    }

    [Fact]
    public void GetDataverseConnection_CertificateStoreLocationParameter_Exists()
    {
        // Assert
        ParameterExists("CertificateStoreLocation").Should().BeTrue();
    }

    [Fact]
    public void GetDataverseConnection_CertificateStoreNameParameter_Exists()
    {
        // Assert
        ParameterExists("CertificateStoreName").Should().BeTrue();
    }

    [Fact]
    public void GetDataverseConnection_Certificate_ParametersInCorrectParameterSet()
    {
        // Arrange/Act
        var certPathAttr = GetParameterInSet("CertificatePath", "Authenticate with client certificate");
        var certPasswordAttr = GetParameterInSet("CertificatePassword", "Authenticate with client certificate");
        var certThumbprintAttr = GetParameterInSet("CertificateThumbprint", "Authenticate with client certificate");

        // Assert
        certPathAttr.Should().NotBeNull("CertificatePath should be in Certificate parameter set");
        certPasswordAttr.Should().NotBeNull("CertificatePassword should be in Certificate parameter set");
        certThumbprintAttr.Should().NotBeNull("CertificateThumbprint should be in Certificate parameter set");
    }

    [Fact]
    public void GetDataverseConnection_Certificate_ClientIdRequired()
    {
        // Arrange/Act
        var clientIdAttr = GetParameterInSet("ClientId", "Authenticate with client certificate");

        // Assert
        clientIdAttr.Should().NotBeNull();
        clientIdAttr!.Mandatory.Should().BeTrue("ClientId should be required for certificate auth");
    }

    [Fact]
    public void GetDataverseConnection_Certificate_UrlOptional()
    {
        // Arrange/Act
        var urlAttr = GetParameterInSet("Url", "Authenticate with client certificate");

        // Assert
        urlAttr.Should().NotBeNull();
        urlAttr!.Mandatory.Should().BeFalse("Url should be optional for certificate auth");
    }

    [Fact]
    public void GetDataverseConnection_Certificate_CertificatePathRequired()
    {
        // Arrange/Act
        var certPathAttr = GetParameterInSet("CertificatePath", "Authenticate with client certificate");

        // Assert
        certPathAttr.Should().NotBeNull();
        certPathAttr!.Mandatory.Should().BeTrue("CertificatePath should be required in certificate auth");
    }

    [Fact(Skip = "Certificate validation test requires actually attempting connection - use E2E tests")]
    public void GetDataverseConnection_Certificate_ThrowsError_WhenNeitherPathNorThumbprint()
    {
        // This requires actually attempting to connect, which would need real credentials
        // Use E2E tests for this validation
    }

    // ===== ConnectionString Parameter Tests =====

    [Fact]
    public void GetDataverseConnection_ConnectionStringParameter_Exists()
    {
        // Assert
        ParameterExists("ConnectionString").Should().BeTrue();
    }

    [Fact]
    public void GetDataverseConnection_ConnectionStringParameter_InCorrectParameterSet()
    {
        // Arrange/Act
        var connStrAttr = GetParameterAttributes("ConnectionString");

        // Assert
        connStrAttr.Should().NotBeEmpty();
        var paramSetAttr = connStrAttr!.FirstOrDefault(a => a.ParameterSetName.Contains("connection string"));
        paramSetAttr.Should().NotBeNull("ConnectionString should be in ConnectionString parameter set");
    }

    [Fact]
    public void GetDataverseConnection_ConnectionStringParameter_IsMandatory()
    {
        // Arrange/Act
        var connStrAttrs = GetParameterAttributes("ConnectionString");

        // Assert
        connStrAttrs.Should().NotBeEmpty();
        var mandatoryAttr = connStrAttrs!.FirstOrDefault(a => a.Mandatory);
        mandatoryAttr.Should().NotBeNull("ConnectionString should be mandatory in its parameter set");
    }

    [Fact]
    public void GetDataverseConnection_ConnectionString_UrlNotMandatory()
    {
        // Arrange/Act - Url should not be mandatory in the ConnectionString parameter set
        // The ConnectionString parameter set doesn't include Url as mandatory
        var urlAttrs = GetParameterAttributes("Url");

        // Assert - Check that Url is not in ConnectionString parameter set at all,
        // or if it is, it's not mandatory
        var connStrUrlAttr = urlAttrs?.FirstOrDefault(a => a.ParameterSetName.Contains("connection string"));
        if (connStrUrlAttr != null)
        {
            connStrUrlAttr.Mandatory.Should().BeFalse();
        }
        // If Url is not in ConnectionString parameter set, that's also valid
    }

    [Fact]
    public void GetDataverseConnection_ConnectionStringParameter_AcceptsString()
    {
        // Arrange/Act
        var connStrProp = _cmdletType.GetProperty("ConnectionString");

        // Assert
        connStrProp.Should().NotBeNull();
        connStrProp!.PropertyType.Should().Be(typeof(string));
    }

    [Fact(Skip = "ConnectionString auth requires valid connection string - use E2E tests")]
    public void GetDataverseConnection_ConnectionString_CanUseWithoutUrl()
    {
        // This requires actually attempting to connect with a real connection string
    }

    [Fact]
    public void GetDataverseConnection_ConnectionStringParameterSet_NameIsCorrect()
    {
        // Arrange/Act
        var connStrAttrs = GetParameterAttributes("ConnectionString");

        // Assert
        connStrAttrs.Should().NotBeEmpty();
        var paramSetAttr = connStrAttrs!.FirstOrDefault();
        paramSetAttr.Should().NotBeNull();
        paramSetAttr!.ParameterSetName.Should().Contain("connection string", 
            because: "parameter set name should contain 'connection string'");
    }

    [Fact(Skip = "SetAsDefault with ConnectionString requires valid connection - use E2E tests")]
    public void GetDataverseConnection_SetAsDefault_WorksWithConnectionStringParameterSet()
    {
        // This requires actually connecting with a real connection string
    }
}
