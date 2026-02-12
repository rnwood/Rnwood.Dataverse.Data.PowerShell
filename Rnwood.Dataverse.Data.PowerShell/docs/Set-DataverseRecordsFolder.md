---
external help file: Set-DataverseRecordsFolder.psm1-help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseRecordsFolder

## SYNOPSIS
Writes a list of Dataverse records to a folder of JSON files.

## SYNTAX

```
Set-DataverseRecordsFolder [-OutputPath] <String> [[-InputObject] <PSObject>] [[-Connection] <Object>]
 [-withdeletions] [[-idproperties] <String[]>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Writes a list of Dataverse records to a folder where each file represents a single record. The files are named using the `Id` property.

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseRecord -tablename contact | Set-DataverseRecordsFolder data/contacts
```

Writes all contacts to the folder `data/contacts`.

## PARAMETERS

### -Connection
Dataverse connection to use for downloading files. Required if records contain DataverseFileReference properties.

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -idproperties
Specifies the list of properties that will be used to generate a unique name for each file. By default this is "Id".

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 4
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
Dataverse record(s) to write. Generally should be piped in from the pipeline.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -OutputPath
Path to write output to

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
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

### -withdeletions
Output a list of deletions (records that were there last time, but are no longer present in the inputs) to `deletions` subfolder of output

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
