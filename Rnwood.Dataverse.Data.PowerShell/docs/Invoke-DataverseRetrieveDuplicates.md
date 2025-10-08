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
Invoke-DataverseRetrieveDuplicates [-BusinessEntity <PSObject>] [-BusinessEntityTableName <String>]
 [-BusinessEntityIgnoreProperties <String[]>] [-BusinessEntityLookupColumns <Hashtable>]
 [-MatchingEntityName <String>] [-PagingInfo <PagingInfo>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to detect and retrieve duplicates for a specified record.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveDuplicates -Connection <ServiceClient> -BusinessEntity <PSObject> -BusinessEntityTableName <String> -BusinessEntityIgnoreProperties <String[]> -BusinessEntityLookupColumns <Hashtable> -MatchingEntityName <String> -PagingInfo <PagingInfo>
```

## PARAMETERS

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

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

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
Gets or sets a paging information for the retrieved duplicates. Required.

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

### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
