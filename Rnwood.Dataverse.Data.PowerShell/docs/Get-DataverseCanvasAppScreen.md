---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseCanvasAppScreen

## SYNOPSIS
Retrieves screens from a Canvas app's .msapp file.

## SYNTAX

### FromDataverse
```
Get-DataverseCanvasAppScreen -CanvasAppId <Guid> [-CanvasAppName <String>] [-ScreenName <String>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### FromFile
```
Get-DataverseCanvasAppScreen -MsAppPath <String> [-ScreenName <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Retrieves screens from a Canvas app's .msapp file. You can retrieve screens from a Canvas app in Dataverse or from a local .msapp file. Screens are returned as PowerShell objects containing the screen name, file path, and YAML content.

## EXAMPLES

### Example 1: Get all screens from a Canvas app in Dataverse
```powershell
PS C:\> $screens = Get-DataverseCanvasAppScreen -CanvasAppId "12345678-1234-1234-1234-123456789012"
PS C:\> $screens | Format-Table ScreenName, Size
```

Retrieves all screens from a Canvas app in Dataverse.

### Example 2: Get screens from a local .msapp file
```powershell
PS C:\> $screens = Get-DataverseCanvasAppScreen -MsAppPath "C:\apps\myapp.msapp"
PS C:\> $screens[0].YamlContent  # View the YAML content of the first screen
```

Retrieves screens from a local .msapp file.

### Example 3: Filter screens by name pattern
```powershell
PS C:\> Get-DataverseCanvasAppScreen -MsAppPath "C:\apps\myapp.msapp" -ScreenName "Main*"
```

Retrieves only screens whose names start with "Main".

## PARAMETERS

### -CanvasAppId
ID of the Canvas app to retrieve screens from

```yaml
Type: Guid
Parameter Sets: FromDataverse
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CanvasAppName
Name of the Canvas app to retrieve screens from

```yaml
Type: String
Parameter Sets: FromDataverse
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g.
http://server.com/MyOrg/).
If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MsAppPath
Path to an .msapp file to read screens from

```yaml
Type: String
Parameter Sets: FromFile
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

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

### -ScreenName
Name pattern of screens to retrieve.
Supports wildcards (* and ?)

```yaml
Type: String
Parameter Sets: (All)
Aliases:

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

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS
