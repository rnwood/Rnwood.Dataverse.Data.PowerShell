---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseGetDefaultPriceLevel

## SYNOPSIS
Contains the data that is needed to retrieve the default price level (price list) for the current user based on the user's territory relationship with the price level.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetDefaultPriceLevelRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GetDefaultPriceLevelRequest)

## SYNTAX

```
Invoke-DataverseGetDefaultPriceLevel -Connection <ServiceClient> -EntityName <String> -ColumnSet <ColumnSet>
```

## DESCRIPTION
Contains the data that is needed to retrieve the default price level (price list) for the current user based on the user's territory relationship with the price level.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseGetDefaultPriceLevel -Connection <ServiceClient> -EntityName <String> -ColumnSet <ColumnSet>
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

### -EntityName
Gets or sets the logical name of the entity.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.GetDefaultPriceLevelResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetDefaultPriceLevelResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GetDefaultPriceLevelResponse)
## NOTES

## RELATED LINKS
