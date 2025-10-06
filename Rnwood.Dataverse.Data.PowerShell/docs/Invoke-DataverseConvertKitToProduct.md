---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseConvertKitToProduct

## SYNOPSIS
Deprecated. Contains the data that is needed to convert a kit to a product.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ConvertKitToProductRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ConvertKitToProductRequest)

## SYNTAX

```
Invoke-DataverseConvertKitToProduct -Connection <ServiceClient> -KitId <Guid>
```

## DESCRIPTION
Deprecated. Contains the data that is needed to convert a kit to a product.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseConvertKitToProduct -Connection <ServiceClient> -KitId <Guid>
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

### -KitId
Gets or sets the ID of the kit. Required.

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

### Microsoft.Crm.Sdk.Messages.ConvertKitToProductResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ConvertKitToProductResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ConvertKitToProductResponse)
## NOTES

## RELATED LINKS
