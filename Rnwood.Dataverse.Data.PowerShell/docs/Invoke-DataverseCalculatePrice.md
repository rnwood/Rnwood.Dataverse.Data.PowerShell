---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCalculatePrice

## SYNOPSIS
Contains the data that is needed to calculate price in an opportunity, quote, order, and invoice. This is used internally for custom pricing calculation when the default system pricing is overridden.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CalculatePriceRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CalculatePriceRequest)

## SYNTAX

```
Invoke-DataverseCalculatePrice -Connection <ServiceClient> -Target <PSObject> -ParentId <Guid>
```

## DESCRIPTION
Contains the data that is needed to calculate price in an opportunity, quote, order, and invoice. This is used internally for custom pricing calculation when the default system pricing is overridden.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCalculatePrice -Connection <ServiceClient> -Target <PSObject> -ParentId <Guid>
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

### -ParentId
For internal use only.

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

### Microsoft.Crm.Sdk.Messages.CalculatePriceResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CalculatePriceResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CalculatePriceResponse)
## NOTES

## RELATED LINKS
