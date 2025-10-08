---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseFulfillSalesOrder

## SYNOPSIS
Contains the data that is needed to fulfill the sales order (order).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.FulfillSalesOrderRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.FulfillSalesOrderRequest)

## SYNTAX

```
Invoke-DataverseFulfillSalesOrder -Connection <ServiceClient> -OrderClose <PSObject> -OrderCloseTableName <String> -OrderCloseIgnoreProperties <String[]> -OrderCloseLookupColumns <Hashtable> -Status <OptionSetValue>
```

## DESCRIPTION
Contains the data that is needed to fulfill the sales order (order).

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseFulfillSalesOrder -Connection <ServiceClient> -OrderClose <PSObject> -OrderCloseTableName <String> -OrderCloseIgnoreProperties <String[]> -OrderCloseLookupColumns <Hashtable> -Status <OptionSetValue>
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

### -OrderClose
Gets or sets the order close activity associated with the sales order (order) to be fulfilled. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type.

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

### -OrderCloseTableName
Gets or sets the order close activity associated with the sales order (order) to be fulfilled. Required. The logical name of the table/entity type for the OrderClose parameter.

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

### -OrderCloseIgnoreProperties
Gets or sets the order close activity associated with the sales order (order) to be fulfilled. Required. Properties to ignore when converting OrderClose PSObject to Entity.

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

### -OrderCloseLookupColumns
Gets or sets the order close activity associated with the sales order (order) to be fulfilled. Required. Hashtable specifying lookup columns for entity reference conversions in OrderClose.

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

### -Status
Gets or sets a status of the sales order (order). Required.

```yaml
Type: OptionSetValue
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

### Microsoft.Crm.Sdk.Messages.FulfillSalesOrderResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.FulfillSalesOrderResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.FulfillSalesOrderResponse)
## NOTES

## RELATED LINKS
