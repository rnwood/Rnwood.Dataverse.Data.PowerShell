---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseGetQuantityDecimal

## SYNOPSIS
Contains the data that is needed to get the quantity decimal value of a product for the specified entity in the target.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetQuantityDecimalRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GetQuantityDecimalRequest)

## SYNTAX

```
Invoke-DataverseGetQuantityDecimal -Connection <ServiceClient> -Target <PSObject> -ProductId <Guid> -UoMId <Guid>
```

## DESCRIPTION
Contains the data that is needed to get the quantity decimal value of a product for the specified entity in the target.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseGetQuantityDecimal -Connection <ServiceClient> -Target <PSObject> -ProductId <Guid> -UoMId <Guid>
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
Gets or sets the target record for this request. Required. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### -ProductId
Gets or sets the ID of the product. Required.

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

### -UoMId
Gets or sets the ID of the unit of measure (unit). Required.

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

### Microsoft.Crm.Sdk.Messages.GetQuantityDecimalResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetQuantityDecimalResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GetQuantityDecimalResponse)
## NOTES

## RELATED LINKS
