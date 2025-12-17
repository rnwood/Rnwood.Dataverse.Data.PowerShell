---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseIconSetIcon

## SYNOPSIS
Retrieves available icons from supported online icon sets.

## SYNTAX

```
Get-DataverseIconSetIcon [[-IconSet] <String>] [[-Name] <String>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseIconSetIcon cmdlet retrieves a list of available icons from supported online icon sets (FluentUI, Tabler, Iconoir). It can list all icons or filter them by name using wildcards. This cmdlet is useful for browsing available icons before setting them on tables using Set-DataverseTableIconFromSet.

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-DataverseIconSetIcon
```

Lists all available icons from the default FluentUI icon set.

## PARAMETERS

### -IconSet
Icon set to retrieve icons from

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: FluentUI, Iconoir, Tabler

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Filter pattern to match icon names (supports wildcards like 'user*' or '*settings')

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
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
