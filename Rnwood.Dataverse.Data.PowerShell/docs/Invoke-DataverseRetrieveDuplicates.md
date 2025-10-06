---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveDuplicates

## SYNOPSIS
Contains the data that is needed to detect and retrieve duplicates for a specified record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveDuplicatesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveDuplicatesRequest)

## SYNTAX

```
Invoke-DataverseRetrieveDuplicates -Connection <ServiceClient> -BusinessEntity <PSObject> -BusinessEntityTableName <String> -BusinessEntityIgnoreProperties <String[]> -BusinessEntityLookupColumns <Hashtable> -MatchingEntityName <String> -PagingInfo <PagingInfo>
```

## DESCRIPTION
Contains the data that is needed to detect and retrieve duplicates for a specified record.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveDuplicates -Connection <ServiceClient> -BusinessEntity <PSObject> -BusinessEntityTableName <String> -BusinessEntityIgnoreProperties <String[]> -BusinessEntityLookupColumns <Hashtable> -MatchingEntityName <String> -PagingInfo <PagingInfo>
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

### -BusinessEntity
Gets or sets a record for which the duplicates are retrieved. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type.

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

### -BusinessEntityTableName
Gets or sets a record for which the duplicates are retrieved. Required. The logical name of the table/entity type for the BusinessEntity parameter.

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

### -BusinessEntityIgnoreProperties
Gets or sets a record for which the duplicates are retrieved. Required. Properties to ignore when converting BusinessEntity PSObject to Entity.

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

### -BusinessEntityLookupColumns
Gets or sets a record for which the duplicates are retrieved. Required. Hashtable specifying lookup columns for entity reference conversions in BusinessEntity.

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

### -MatchingEntityName
Gets or sets a name of the matching entity type. Required.

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

### -PagingInfo
Gets or sets the paging information. Optional.

```yaml
Type: PagingInfo
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

### Microsoft.Crm.Sdk.Messages.RetrieveDuplicatesResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveDuplicatesResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveDuplicatesResponse)
## NOTES

## RELATED LINKS
