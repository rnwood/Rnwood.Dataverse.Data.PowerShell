---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieve

## SYNOPSIS
Contains the data that is needed to retrieve a record.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveRequest)

## SYNTAX

```
Invoke-DataverseRetrieve -Connection <ServiceClient> -Target <PSObject> [-ColumnSet <ColumnSet>]
 [-RelatedEntitiesQuery <RelationshipQueryCollection>] [-ReturnNotifications <Boolean>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to retrieve a record.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieve -Connection <ServiceClient> -Target <PSObject> -ColumnSet <ColumnSet> -RelatedEntitiesQuery <RelationshipQueryCollection> -ReturnNotifications <Boolean>
```

## PARAMETERS

### -ColumnSet
Gets or sets the collection of columns for which non-null values are returned from a query. Required.

```yaml
Type: ColumnSet
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
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

### -RelatedEntitiesQuery
Gets or sets the query that describes the related records to be retrieved. Optional.

```yaml
Type: RelationshipQueryCollection
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ReturnNotifications
For internal use only.

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

### -Target
Gets or sets the target, which is a recurring appointment master record to which the appointment is converted. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
