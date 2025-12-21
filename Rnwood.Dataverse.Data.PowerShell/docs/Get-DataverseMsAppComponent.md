---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseMsAppComponent

## SYNOPSIS
Retrieves components from a .msapp file.

## SYNTAX

```
Get-DataverseMsAppComponent [-MsAppPath] <String> [-ComponentName <String>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Retrieves components from a Canvas app .msapp file. Components are reusable custom controls defined in YAML format.

## EXAMPLES

### Example 1: Get all components from a .msapp file
```powershell
PS C:\> $components = Get-DataverseMsAppComponent -MsAppPath "C:\apps\myapp.msapp"
PS C:\> $components | Format-Table ComponentName, Size
```

Retrieves all components from a .msapp file.

### Example 2: Filter components by name
```powershell
PS C:\> Get-DataverseMsAppComponent -MsAppPath "myapp.msapp" -ComponentName "Custom*"
```

Retrieves only components whose names start with "Custom".

## PARAMETERS

### -ComponentName
Name pattern of components to retrieve.
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

### -MsAppPath
Path to the .msapp file

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS
