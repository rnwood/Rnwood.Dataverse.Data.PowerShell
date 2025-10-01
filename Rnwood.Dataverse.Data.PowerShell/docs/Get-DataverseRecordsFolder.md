---
external help file: Get-DataverseRecordsFolder.psm1-help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseRecordsFolder

## SYNOPSIS
Reads a folder of JSON files written out by `Set-DataverseRecordFolder` and converts back into a stream of PS objects.
Together these commands can be used to extract and import data to and from files, for instance for inclusion in source control, or build/deployment assets.

## SYNTAX

```
Get-DataverseRecordsFolder [-InputPath] <String> [-deletions] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-DataversRecordsFolder -InputPath data/contacts | Set-DataverseRecord -connection $c
```

Reads files from `data/contacts` and uses them to create/update records in Dataverse using the existing connection `$c`.
See documentation for `Set-DataverseRecord` as there are option to control how/if existing records will be matched and updated.

## PARAMETERS

### -InputPath
Path to folder to read JSON files from.

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

### -deletions
{{ Fill deletions Description }}

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

### -ProgressAction
See standard PS docs.

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

### System.Object
## NOTES

## RELATED LINKS
