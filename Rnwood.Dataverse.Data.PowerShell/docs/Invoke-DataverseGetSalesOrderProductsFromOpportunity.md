---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseGetSalesOrderProductsFromOpportunity

## SYNOPSIS
Contains the data that is needed to retrieve the products from an opportunity and copy them to the sales order (order).For the Web API use GetSalesOrderProductsFromOpportunity Action.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetSalesOrderProductsFromOpportunityRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GetSalesOrderProductsFromOpportunityRequest)

## SYNTAX

```
Invoke-DataverseGetSalesOrderProductsFromOpportunity -Connection <ServiceClient> -OpportunityId <Guid> -SalesOrderId <Guid>
```

## DESCRIPTION
Contains the data that is needed to retrieve the products from an opportunity and copy them to the sales order (order).For the Web API use GetSalesOrderProductsFromOpportunity Action.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseGetSalesOrderProductsFromOpportunity -Connection <ServiceClient> -OpportunityId <Guid> -SalesOrderId <Guid>
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

### -OpportunityId
Gets or sets the ID of the opportunity.

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

### -SalesOrderId
Gets or sets the ID of the sales order (order).

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.GetSalesOrderProductsFromOpportunityResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetSalesOrderProductsFromOpportunityResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GetSalesOrderProductsFromOpportunityResponse)
## NOTES

## RELATED LINKS
