---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseMerge

## SYNOPSIS
Contains the data that’s needed to merge the information from two entity records of the same type.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.MergeRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.MergeRequest)

## SYNTAX

```
Invoke-DataverseMerge -Connection <ServiceClient> -Target <PSObject> -SubordinateId <Guid> -UpdateContent <PSObject> -UpdateContentTableName <String> -UpdateContentIgnoreProperties <String[]> -UpdateContentLookupColumns <Hashtable> -PerformParentingChecks <Boolean>
```

## DESCRIPTION
Contains the data that’s needed to merge the information from two entity records of the same type.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseMerge -Connection <ServiceClient> -Target <PSObject> -SubordinateId <Guid> -UpdateContent <PSObject> -UpdateContentTableName <String> -UpdateContentIgnoreProperties <String[]> -UpdateContentLookupColumns <Hashtable> -PerformParentingChecks <Boolean>
```

## PARAMETERS

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

### -Target
Gets or sets the target, which is a recurring appointment master record to which the appointment is converted. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True
Accept wildcard characters: False
```

### -SubordinateId
Gets or sets the ID of the entity record from which to merge data. Required.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UpdateContent
Gets or sets additional entity attributes to be set during the merge operation for accounts, contacts, or leads. This property is not applied when merging incidents. Optional. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UpdateContentTableName
Gets or sets additional entity attributes to be set during the merge operation for accounts, contacts, or leads. This property is not applied when merging incidents. Optional. The logical name of the table/entity type for the UpdateContent parameter.

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

### -UpdateContentIgnoreProperties
Gets or sets additional entity attributes to be set during the merge operation for accounts, contacts, or leads. This property is not applied when merging incidents. Optional. Properties to ignore when converting UpdateContent PSObject to Entity.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UpdateContentLookupColumns
Gets or sets additional entity attributes to be set during the merge operation for accounts, contacts, or leads. This property is not applied when merging incidents. Optional. Hashtable specifying lookup columns for entity reference conversions in UpdateContent.

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PerformParentingChecks
Gets or sets a value that indicates whether to check if the parent information is different for the two entity records. Required.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.MergeResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.MergeResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.MergeResponse)
## NOTES

## RELATED LINKS
