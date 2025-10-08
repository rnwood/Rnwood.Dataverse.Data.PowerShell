---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseGenerateInvoiceFromOpportunity

## SYNOPSIS
Contains the data that is needed to generate an invoice from an opportunity.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GenerateInvoiceFromOpportunityRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GenerateInvoiceFromOpportunityRequest)

## SYNTAX

```
Invoke-DataverseGenerateInvoiceFromOpportunity -Connection <ServiceClient> -OpportunityId <Guid> -ColumnSet <ColumnSet>
```

## DESCRIPTION
Contains the data that is needed to generate an invoice from an opportunity.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseGenerateInvoiceFromOpportunity -Connection <ServiceClient> -OpportunityId <Guid> -ColumnSet <ColumnSet>
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
Gets or sets the ID of the opportunity to be used as the basis for the new invoice. Required.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.GenerateInvoiceFromOpportunityResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GenerateInvoiceFromOpportunityResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GenerateInvoiceFromOpportunityResponse)
## NOTES

## RELATED LINKS
