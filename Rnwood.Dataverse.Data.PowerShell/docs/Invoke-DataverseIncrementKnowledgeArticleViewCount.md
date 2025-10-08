---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseIncrementKnowledgeArticleViewCount

## SYNOPSIS
Contains the data that is required to increment the per day view count of a knowledge article record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.IncrementKnowledgeArticleViewCountRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.IncrementKnowledgeArticleViewCountRequest)

## SYNTAX

```
Invoke-DataverseIncrementKnowledgeArticleViewCount -Connection <ServiceClient> -Source <PSObject> -ViewDate <DateTime> -Location <Int32> -Count <Int32>
```

## DESCRIPTION
Contains the data that is required to increment the per day view count of a knowledge article record.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseIncrementKnowledgeArticleViewCount -Connection <ServiceClient> -Source <PSObject> -ViewDate <DateTime> -Location <Int32> -Count <Int32>
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

### -Source
Gets or sets the knowledge article record for incrementing the view count. Required. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### -ViewDate
Gets or sets the date for which the view count has to be incremented.

```yaml
Type: DateTime
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Location
Gets or sets the location where the knowledge article record was used.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Count
Gets the number of elements in the collection.

```yaml
Type: Int32
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

### Microsoft.Crm.Sdk.Messages.IncrementKnowledgeArticleViewCountResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.IncrementKnowledgeArticleViewCountResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.IncrementKnowledgeArticleViewCountResponse)
## NOTES

## RELATED LINKS
