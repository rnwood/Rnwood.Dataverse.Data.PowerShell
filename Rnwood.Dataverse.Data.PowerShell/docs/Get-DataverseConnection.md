---
external help file: Rnwood.Dataverse.Data.PowerShell.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseConnection

## SYNOPSIS
Gets a connection to a Dataverse environment either interactively or silently.

## SYNTAX

### Authenticate with client secret
```
Get-DataverseConnection -ClientId <Guid> -Url <Uri> -ClientSecret <String> [<CommonParameters>]
```

### Authenticate interactively
```
Get-DataverseConnection [-ClientId <Guid>] -Url <Uri> [-Username <String>] [-Interactive] [<CommonParameters>]
```

### Authenticate using the device code flow
```
Get-DataverseConnection [-ClientId <Guid>] -Url <Uri> [-Username <String>] [-DeviceCode] [<CommonParameters>]
```

### Authenticate with username and password
```
Get-DataverseConnection [-ClientId <Guid>] -Url <Uri> -Username <String> -Password <String>
 [<CommonParameters>]
```

## DESCRIPTION

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

### -Url
{{ Fill Url Description }}

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
{{ Fill Username Description }}

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

### -ClientId
{{ Fill ClientId Description }}

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
{{ Fill ClientSecret Description }}

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
{{ Fill DeviceCode Description }}

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
{{ Fill Interactive Description }}

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

### -Password
{{ Fill Password Description }}

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

### System.Object
## NOTES

## RELATED LINKS
