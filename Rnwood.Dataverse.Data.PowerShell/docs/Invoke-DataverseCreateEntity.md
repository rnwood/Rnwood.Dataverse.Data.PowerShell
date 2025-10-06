---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCreateEntity

## SYNOPSIS
Contains the data that is needed to create a table, and optionally, to add it to a specified unmanaged solution.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.CreateEntityRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.CreateEntityRequest)

## SYNTAX

```
Invoke-DataverseCreateEntity -Connection <ServiceClient> [-Entity <EntityMetadata>] [-HasActivities <Boolean>]
 [-HasNotes <Boolean>] [-HasFeedback <Boolean>] [-PrimaryAttribute <StringAttributeMetadata>]
 [-SolutionUniqueName <String>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to create a table, and optionally, to add it to a specified unmanaged solution.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCreateEntity -Connection <ServiceClient> -Entity <EntityMetadata> -HasActivities <Boolean> -HasNotes <Boolean> -HasFeedback <Boolean> -PrimaryAttribute <StringAttributeMetadata> -SolutionUniqueName <String>
```

## PARAMETERS

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Entity
Gets or sets the duplicate rule that you want updated. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type.

```yaml
Type: EntityMetadata
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HasActivities
Gets or sets whether the table is created that has a special relationship to activity entities a nd is a valid regarding object for the activity. Optional.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HasFeedback
Gets or sets whether the table will have a special relationship to the Feedback table. Optional.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HasNotes
Gets or sets whether the custom table that is created has a special relationship to the annotation table. Optional.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrimaryAttribute
Gets or sets the metadata for the primary name column for the new table. Required.

```yaml
Type: StringAttributeMetadata
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SolutionUniqueName
Gets or sets the name of the unmanaged solution to which you want to add this column. Optional.

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

### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: False
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

### System.Object
## NOTES

## RELATED LINKS
