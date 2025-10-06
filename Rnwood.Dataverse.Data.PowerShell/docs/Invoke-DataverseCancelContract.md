---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCancelContract

## SYNOPSIS
Contains the data that is needed to cancel a contract.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CancelContractRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CancelContractRequest)

## SYNTAX

```
Invoke-DataverseCancelContract -Connection <ServiceClient> -ContractId <Guid> -CancelDate <DateTime> -Status <OptionSetValue>
```

## DESCRIPTION
Contains the data that is needed to cancel a contract.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCancelContract -Connection <ServiceClient> -ContractId <Guid> -CancelDate <DateTime> -Status <OptionSetValue>
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
Gets or sets the ID of the contract. Required.

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

### -CancelDate
Gets or sets the contract cancellation date. Required.

```yaml
Type: DateTime
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Status
Gets or sets the status of the contract. Required.

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

### Microsoft.Crm.Sdk.Messages.CancelContractResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CancelContractResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CancelContractResponse)
## NOTES

## RELATED LINKS
