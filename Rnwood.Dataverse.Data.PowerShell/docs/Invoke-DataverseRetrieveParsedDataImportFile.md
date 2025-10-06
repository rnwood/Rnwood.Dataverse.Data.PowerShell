---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveParsedDataImportFile

## SYNOPSIS
Contains the data that is needed to retrieve the data from the parse table.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveParsedDataImportFileRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveParsedDataImportFileRequest)

## SYNTAX

```
Invoke-DataverseRetrieveParsedDataImportFile -Connection <ServiceClient> -ImportFileId <Guid> -PagingInfo <PagingInfo>
```

## DESCRIPTION
Contains the data that is needed to retrieve the data from the parse table.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveParsedDataImportFile -Connection <ServiceClient> -ImportFileId <Guid> -PagingInfo <PagingInfo>
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

### -ImportFileId
Gets or sets the ID of the import file that is associated with the parse table. Required.

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

### -PagingInfo
Gets or sets the paging information. Optional.

```yaml
Type: PagingInfo
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

### Microsoft.Crm.Sdk.Messages.RetrieveParsedDataImportFileResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveParsedDataImportFileResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveParsedDataImportFileResponse)
## NOTES

## RELATED LINKS
