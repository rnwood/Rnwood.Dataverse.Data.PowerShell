---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveFormattedImportJobResults

## SYNOPSIS
Contains the data that is needed to retrieve the formatted results from an import job.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveFormattedImportJobResultsRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveFormattedImportJobResultsRequest)

## SYNTAX

```
Invoke-DataverseRetrieveFormattedImportJobResults -Connection <ServiceClient> -ImportJobId <Guid>
```

## DESCRIPTION
Contains the data that is needed to retrieve the formatted results from an import job.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveFormattedImportJobResults -Connection <ServiceClient> -ImportJobId <Guid>
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

### -ImportJobId
Gets or sets the GUID of an import job. Required.

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

### Microsoft.Crm.Sdk.Messages.RetrieveFormattedImportJobResultsResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveFormattedImportJobResultsResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveFormattedImportJobResultsResponse)
## NOTES

## RELATED LINKS
