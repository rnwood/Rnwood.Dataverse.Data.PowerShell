---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseConnection

## SYNOPSIS
Gets a connection to a Dataverse environment either interactively or silently and returns it.

All commands that need a connection to Dataverse expect you to provide the connection in `-connection` parameter.
So you can store the output of this command in a variable and pass it to each command that needs it.
See the examples for this pattern below.

## SYNTAX

### Get default connection
```
Get-DataverseConnection [-GetDefault] [-SetAsDefault] [-Timeout <UInt32>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### Return a mock connection
```
Get-DataverseConnection [-SetAsDefault] -Mock <EntityMetadata[]> [-RequestInterceptor <ScriptBlock>] -Url <Uri>
 [-Timeout <UInt32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate with client secret
```
Get-DataverseConnection [-SetAsDefault] -ClientId <Guid> -Url <Uri> -ClientSecret <String> [-Timeout <UInt32>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate interactively
```
Get-DataverseConnection [-SetAsDefault] [-ClientId <Guid>] [-Url <Uri>] [-Username <String>] [-Interactive]
 [-Timeout <UInt32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate using the device code flow
```
Get-DataverseConnection [-SetAsDefault] [-ClientId <Guid>] [-Url <Uri>] [-Username <String>] [-DeviceCode]
 [-Timeout <UInt32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate with username and password
```
Get-DataverseConnection [-SetAsDefault] [-ClientId <Guid>] [-Url <Uri>] -Username <String> -Password <String>
 [-Timeout <UInt32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate with Dataverse SDK connection string.
```
Get-DataverseConnection [-SetAsDefault] -Url <Uri> -ConnectionString <String> [-Timeout <UInt32>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate with DefaultAzureCredential
```
Get-DataverseConnection [-SetAsDefault] [-Url <Uri>] [-DefaultAzureCredential] [-Timeout <UInt32>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate with ManagedIdentityCredential
```
Get-DataverseConnection [-SetAsDefault] [-Url <Uri>] [-ManagedIdentity] [-ManagedIdentityClientId <String>]
 [-Timeout <UInt32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate with access token script block
```
Get-DataverseConnection [-SetAsDefault] -Url <Uri> -AccessToken <ScriptBlock> [-Timeout <UInt32>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet establishes a connection to a Microsoft Dataverse environment which can then be used with other cmdlets in this module.

All commands that need a connection to Dataverse expect you to provide the connection in `-connection` parameter.
So you can store the output of this command in a variable and pass it to each command that needs it.
See the examples for this pattern below.

Multiple authentication methods are supported:
- Interactive authentication (browser-based)
- Device code flow (for remote/headless scenarios)
- Username/password
- Client secret (for service principal authentication)
- DefaultAzureCredential (automatic credential discovery for Azure environments)
- ManagedIdentityCredential (for Azure managed identity authentication)
- Connection string (for advanced scenarios)
- Mock connection (for testing)

## EXAMPLES

### Example 1
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -Interactive
```

Gets a connection to MYORG using interactive authentication and stores the result in the `$c` variable for later use.

### Example 2
```powershell
PS C:\> $c = Get-DataverseConnection -url "https://myorg.crm4.dynamics.com" -clientid "3004eb1e-7a00-45e0-a1dc-6703735eac18" -clientsecret "itsasecret"
```

Gets a connection to MYORG using Service Principal client ID and secret auth and stores the result in the `$c` variable for later use.

### Example 3
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -DefaultAzureCredential
```

Gets a connection to MYORG using DefaultAzureCredential, which automatically discovers credentials from the environment (environment variables, managed identity, Visual Studio, Azure CLI, Azure PowerShell, or interactive browser). This is ideal for Azure-hosted applications.

### Example 4
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ManagedIdentity
```

Gets a connection to MYORG using the system-assigned managed identity. This is useful when running in Azure environments like Azure Functions, App Service, or VMs with managed identity enabled.

### Example 5
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ManagedIdentity -ManagedIdentityClientId "12345678-1234-1234-1234-123456789abc"
```

Gets a connection to MYORG using a user-assigned managed identity with the specified client ID.

### Example 6
```powershell
PS C:\> $c = Get-DataverseConnection -Interactive
```

Authenticates interactively without specifying a URL. The cmdlet will automatically display a list of available Dataverse environments for the user to select from. This is useful when you have access to multiple environments and don't want to manually specify the URL.

### Example 7
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -AccessToken { "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im5PbzNaRHJ1VW1sMGhrR2ZaRjBqZWFHWW9XQT..." }
```

Gets a connection to MYORG using a script block that returns an access token. The script block is called whenever a new access token is needed. This is useful for custom authentication scenarios where you manage token acquisition externally.

### Example 8: Save a named connection
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -Interactive -Name "MyOrgProd"
```

Connects to MYORG using interactive authentication and saves the connection with the name "MyOrgProd". The authentication tokens will be cached securely, and connection metadata will be saved for later use.

### Example 9: Load a saved named connection
```powershell
PS C:\> $c = Get-DataverseConnection -Name "MyOrgProd"
```

Loads the previously saved connection named "MyOrgProd". The cmdlet will use the cached authentication tokens, avoiding the need to authenticate again unless the tokens have expired.

### Example 10: List all saved connections
```powershell
PS C:\> Get-DataverseConnection -ListConnections
```

Lists all saved named connections, showing their names, URLs, authentication methods, usernames, and when they were last saved.

### Example 11: Delete a saved connection
```powershell
PS C:\> Get-DataverseConnection -DeleteConnection -Name "MyOrgProd"
```

Deletes the saved connection named "MyOrgProd", removing both the connection metadata and cached authentication tokens.

### Example 12: Save connection with client secret
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "3004eb1e-7a00-45e0-a1dc-6703735eac18" -ClientSecret "itsasecret" -Name "MyOrgService"
```

Connects using client secret authentication and saves the connection as "MyOrgService". Note: The client secret itself is NOT saved for security reasons. When loading this connection later, you will need to provide the client secret again.

## PARAMETERS

### -AccessToken
Script block that returns an access token string. Called whenever a new access token is needed.

```yaml
Type: ScriptBlock
Parameter Sets: Authenticate with access token script block
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ClientId
Client ID to use for authentication. By default the MS provided ID for PAC CLI (`9cee029c-6210-4654-90bb-17e6e9d36617`) is used to make it easy to get started.

```yaml
Type: Guid
Parameter Sets: Authenticate with client secret
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: Guid
Parameter Sets: Authenticate interactively, Authenticate using the device code flow, Authenticate with username and password
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ClientSecret
Client secret to authenticate with, as registered for the Entra ID application.

```yaml
Type: String
Parameter Sets: Authenticate with client secret
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ConnectionString
Specifies the conneciton string to authenticate with - see https://learn.microsoft.com/en-us/power-apps/developer/data-platform/xrm-tooling/use-connection-strings-xrm-tooling-connect

```yaml
Type: String
Parameter Sets: Authenticate with Dataverse SDK connection string.
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DefaultAzureCredential
Use DefaultAzureCredential for authentication. This will try multiple authentication methods in order: environment variables, managed identity, Visual Studio, Azure CLI, Azure PowerShell, and interactive browser.

```yaml
Type: SwitchParameter
Parameter Sets: Authenticate with DefaultAzureCredential
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DeviceCode
Triggers device code authentication where you will be given a URL to visit and a code to complete authentication in web browser.

```yaml
Type: SwitchParameter
Parameter Sets: Authenticate using the device code flow
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -GetDefault
Gets the current default connection. Returns an error if no default connection is set.

```yaml
Type: SwitchParameter
Parameter Sets: Get default connection
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Interactive
Triggers interactive authentication, where browser will be opened for user to interactively log in.

```yaml
Type: SwitchParameter
Parameter Sets: Authenticate interactively
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ManagedIdentity
Use ManagedIdentityCredential for authentication. Authenticates using the managed identity assigned to the Azure resource.

```yaml
Type: SwitchParameter
Parameter Sets: Authenticate with ManagedIdentityCredential
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ManagedIdentityClientId
Client ID of the user-assigned managed identity. If not specified, the system-assigned managed identity will be used.

```yaml
Type: String
Parameter Sets: Authenticate with ManagedIdentityCredential
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Mock
Entity metadata for mock connection. Used for testing purposes. Provide entity metadata objects to configure the mock connection with.

```yaml
Type: EntityMetadata[]
Parameter Sets: Return a mock connection
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Password
Password to authenticate with.

```yaml
Type: String
Parameter Sets: Authenticate with username and password
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RequestInterceptor
ScriptBlock to intercept and modify requests. The ScriptBlock receives the OrganizationRequest and can throw exceptions or return modified responses.

```yaml
Type: ScriptBlock
Parameter Sets: Return a mock connection
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SetAsDefault
When set, this connection will be used as the default for cmdlets that don't have a connection parameter specified.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Timeout
Timeout for authentication operations. Defaults to 5 minutes.

```yaml
Type: UInt32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Url
URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com

```yaml
Type: Uri
Parameter Sets: Return a mock connection, Authenticate with client secret, Authenticate with Dataverse SDK connection string., Authenticate with access token script block
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: Uri
Parameter Sets: Authenticate interactively, Authenticate using the device code flow, Authenticate with username and password, Authenticate with DefaultAzureCredential, Authenticate with ManagedIdentityCredential
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Username
Username to authenticate with.

```yaml
Type: String
Parameter Sets: Authenticate interactively, Authenticate using the device code flow
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: Authenticate with username and password
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
See standard PS documentation.

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### Microsoft.PowerPlatform.Dataverse.Client.ServiceClient
## NOTES

## RELATED LINKS
