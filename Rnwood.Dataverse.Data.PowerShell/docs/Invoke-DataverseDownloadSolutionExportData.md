---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseDownloadSolutionExportData

## SYNOPSIS
Contains the data needed to download a solution file that was exported by an asynchronous job.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DownloadSolutionExportDataRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.DownloadSolutionExportDataRequest)

## SYNTAX

```
Invoke-DataverseDownloadSolutionExportData -Connection <ServiceClient> -ExportJobId <Guid>
```

## DESCRIPTION
Contains the data needed to download a solution file that was exported by an asynchronous job.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseDownloadSolutionExportData -Connection <ServiceClient> -ExportJobId <Guid>
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

### -ExportJobId
The unique identifier of the solution export job.

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

### Microsoft.Crm.Sdk.Messages.DownloadSolutionExportDataResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DownloadSolutionExportDataResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.DownloadSolutionExportDataResponse)
## NOTES

## RELATED LINKS
