---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseDeleteAuditData

## SYNOPSIS
Contains the data that is needed for customers using customer-managed encryption keys to delete all audit data records up until a specified end date.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeleteAuditDataRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.DeleteAuditDataRequest)

## SYNTAX

```
Invoke-DataverseDeleteAuditData -Connection <ServiceClient> -EndDate <DateTime>
```

## DESCRIPTION
Contains the data that is needed for customers using customer-managed encryption keys to delete all audit data records up until a specified end date.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseDeleteAuditData -Connection <ServiceClient> -EndDate <DateTime>
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

### -EndDate
Gets or sets the end date and time. Required.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.DeleteAuditDataResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeleteAuditDataResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.DeleteAuditDataResponse)
## NOTES

## RELATED LINKS
