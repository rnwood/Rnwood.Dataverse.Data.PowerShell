---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveAuditPartitionList

## SYNOPSIS
Contains the data that is needed to retrieve the list of database partitions that are used to store audited history data.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAuditPartitionListRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveAuditPartitionListRequest)

## SYNTAX

```
Invoke-DataverseRetrieveAuditPartitionList -Connection <ServiceClient>
```

## DESCRIPTION
Contains the data that is needed to retrieve the list of database partitions that are used to store audited history data.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveAuditPartitionList -Connection <ServiceClient>
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.RetrieveAuditPartitionListResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAuditPartitionListResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveAuditPartitionListResponse)
## NOTES

## RELATED LINKS
