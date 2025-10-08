---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseUpdateProductProperties

## SYNOPSIS
Contains the data that is needed to update values of the property instances (dynamic property instances) for a product added to an opportunity, quote, order, or invoice.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UpdateProductPropertiesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.UpdateProductPropertiesRequest)

## SYNTAX

```
Invoke-DataverseUpdateProductProperties -Connection <ServiceClient> -PropertyInstanceList <EntityCollection>
```

## DESCRIPTION
Contains the data that is needed to update values of the property instances (dynamic property instances) for a product added to an opportunity, quote, order, or invoice.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseUpdateProductProperties -Connection <ServiceClient> -PropertyInstanceList <EntityCollection>
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

### -PropertyInstanceList
Gets or sets the property instances (dynamic property instances) for a product to be updated.

```yaml
Type: EntityCollection
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

### Microsoft.Crm.Sdk.Messages.UpdateProductPropertiesResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UpdateProductPropertiesResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.UpdateProductPropertiesResponse)
## NOTES

## RELATED LINKS
