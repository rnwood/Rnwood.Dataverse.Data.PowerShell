---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRenewContract

## SYNOPSIS
Contains the data that is needed to renew a contract and create the contract details for a new contract.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RenewContractRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RenewContractRequest)

## SYNTAX

```
Invoke-DataverseRenewContract -Connection <ServiceClient> -ContractId <Guid> -Status <Int32> -IncludeCanceledLines <Boolean>
```

## DESCRIPTION
Contains the data that is needed to renew a contract and create the contract details for a new contract.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRenewContract -Connection <ServiceClient> -ContractId <Guid> -Status <Int32> -IncludeCanceledLines <Boolean>
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

### -ContractId
Gets or sets the ID of the contract to be renewed. Required.

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

### -Status
Gets or sets the status of the contract.

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

### -IncludeCanceledLines
Gets or sets a value that indicates whether the canceled line items of the original contract should be included in the renewed contract. Required.

```yaml
Type: Boolean
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

### Microsoft.Crm.Sdk.Messages.RenewContractResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RenewContractResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RenewContractResponse)
## NOTES

## RELATED LINKS
