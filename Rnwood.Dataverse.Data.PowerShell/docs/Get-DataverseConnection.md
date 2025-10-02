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

### Return a mock connection
```
Get-DataverseConnection -Mock <EntityMetadata[]> -Url <Uri> [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### Authenticate with client secret
```
Get-DataverseConnection -ClientId <Guid> -Url <Uri> -ClientSecret <String> [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### Authenticate interactively
```
Get-DataverseConnection [-ClientId <Guid>] -Url <Uri> [-Username <String>] [-Interactive]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate using the device code flow
```
Get-DataverseConnection [-ClientId <Guid>] -Url <Uri> [-Username <String>] [-DeviceCode]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate with username and password
```
Get-DataverseConnection [-ClientId <Guid>] -Url <Uri> -Username <String> -Password <String>
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate with Dataverse SDK connection string.
```
Get-DataverseConnection -Url <Uri> -ConnectionString <String> [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
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

## PARAMETERS

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

### -Url
URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com

```yaml
Type: Uri
Parameter Sets: (All)
Aliases:

Required: True
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
