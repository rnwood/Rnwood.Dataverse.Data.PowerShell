---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseConvertSalesOrderToInvoice

## SYNOPSIS
Contains the data that is needed to convert a sales order to an invoice.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ConvertSalesOrderToInvoiceRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ConvertSalesOrderToInvoiceRequest)

## SYNTAX

```
Invoke-DataverseConvertSalesOrderToInvoice -Connection <ServiceClient> -SalesOrderId <Guid> -ColumnSet <ColumnSet> -ProcessInstanceId <PSObject>
```

## DESCRIPTION
Contains the data that is needed to convert a sales order to an invoice.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseConvertSalesOrderToInvoice -Connection <ServiceClient> -SalesOrderId <Guid> -ColumnSet <ColumnSet> -ProcessInstanceId <PSObject>
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

### -SalesOrderId
Gets or sets the ID of the sales order (order) to convert. Required.

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

### -ProcessInstanceId
Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.ConvertSalesOrderToInvoiceResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ConvertSalesOrderToInvoiceResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ConvertSalesOrderToInvoiceResponse)
## NOTES

## RELATED LINKS
