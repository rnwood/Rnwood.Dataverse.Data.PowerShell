---
external help file: Rnwood.Dataverse.Data.PowerShell.FrameworkSpecific.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# New-DataverseConnection

## SYNOPSIS
Creates a connection to a Dataverse environment either interactively or silently using a variety of auth types.
	
General information about these authentication types and how to use them with Dataverse can be found here:
https://learn.microsoft.com/en-us/power-apps/developer/data-platform/authenticate-oauth

## SYNTAX

### Use existing SDK connection
```
New-DataverseConnection [-OrganizationService <IOrganizationService>] [<CommonParameters>]
```

### Authenticate with client secret
```
New-DataverseConnection -ClientId <Guid> -Url <Uri> -ClientSecret <String> [<CommonParameters>]
```

### Authenticate interactively
```
New-DataverseConnection [-ClientId <Guid>] -Url <Uri> [-Username <String>] [-Interactive] [<CommonParameters>]
```

### Authenticate using the device code flow
```
New-DataverseConnection [-ClientId <Guid>] -Url <Uri> [-Username <String>] [-DeviceCode] [<CommonParameters>]
```

### Authenticate with username and password
```
New-DataverseConnection [-ClientId <Guid>] -Url <Uri> -Username <String> -Password <String>
 [<CommonParameters>]
```

## DESCRIPTION

## EXAMPLES

### Example 1
```
PS C:\> $c = New-DataverseConnection -Url https://myorg.crm11.dynamics.com -Interactive
```

Gets a connection to MYORG using interactive authentication and stores the result in the \`$c\` variable for later use.

### Example 2
```
PS C:\> $c = New-DataverseConnection -url "https://myorg.crm4.dynamics.com" -clientid "3004eb1e-7a00-45e0-a1dc-6703735eac18" -clientsecret "itsasecret"
```

Gets a connection to MYORG using Service Principal client ID and secret auth and stores the result in the \`$c\` variable for later use.

## PARAMETERS

### -ClientId
Client ID to use for authentication. By default (except for the client secret auth type), the MS provided ID for PAC CLI (`9cee029c-6210-4654-90bb-17e6e9d36617`) is used to make it easy to get started.
		
For the client secret auth type, this parameter is mandatory and you must register an application.

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
Client secret for the registered application.

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

### -DeviceCode
Triggers device code authentication. This is a challenge-response system which directs the user
to authenticate in a web browser on a different device.

```yaml
Type: SwitchParameter
Parameter Sets: Authenticate using the device code flow
Aliases:

Required: True
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Interactive
Triggers interactive authentication. A browser window will open and prompt the user to authenticate.

```yaml
Type: SwitchParameter
Parameter Sets: Authenticate interactively
Aliases:

Required: True
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -OrganizationService
 Specifies an existing IOrganizationService SDK connection to use.

```yaml
Type: IOrganizationService
Parameter Sets: Use existing SDK connection
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Password
Password to authenticate with

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
URL of the Dataverse environment to connect to. This is the root URL for instance https://myorg.crm11.dynamics.com/ and not the URL of a specific API endpoint.


```yaml
Type: Uri
Parameter Sets: Authenticate with client secret, Authenticate interactively, Authenticate using the device code flow, Authenticate with username and password
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### Rnwood.Dataverse.Data.PowerShell.FrameworkSpecific.DataverseConnection
## NOTES

## RELATED LINKS
