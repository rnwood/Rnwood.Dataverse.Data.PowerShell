---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRenewEntitlement

## SYNOPSIS
Contains the data that is needed to renew an entitlement.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RenewEntitlementRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RenewEntitlementRequest)

## SYNTAX

```
Invoke-DataverseRenewEntitlement -Connection <ServiceClient> -EntitlementId <Guid> -Status <Int32>
```

## DESCRIPTION
Contains the data that is needed to renew an entitlement.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRenewEntitlement -Connection <ServiceClient> -EntitlementId <Guid> -Status <Int32>
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

### -EntitlementId
Gets or sets the id of the entitlement to renew.

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
Gets or sets the value for the renewed Entitlement.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.RenewEntitlementResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RenewEntitlementResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RenewEntitlementResponse)
## NOTES

## RELATED LINKS
