---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseGetDistinctValuesImportFile

## SYNOPSIS
Contains the data that is needed to retrieve distinct values from the parse table for a column in the source file that contains list values.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetDistinctValuesImportFileRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GetDistinctValuesImportFileRequest)

## SYNTAX

```
Invoke-DataverseGetDistinctValuesImportFile -Connection <ServiceClient> -ImportFileId <Guid> -columnNumber <Int32> -pageNumber <Int32> -recordsPerPage <Int32>
```

## DESCRIPTION
Contains the data that is needed to retrieve distinct values from the parse table for a column in the source file that contains list values.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseGetDistinctValuesImportFile -Connection <ServiceClient> -ImportFileId <Guid> -columnNumber <Int32> -pageNumber <Int32> -recordsPerPage <Int32>
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
Gets or sets in ID of the import file that is associated with the source file. Required.

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

### -columnNumber
Gets or sets a column number value to identify an attribute from the Audit.AttributeMask column Optional.

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

### -pageNumber
Gets or sets the page number in the source file. Required.

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

### -recordsPerPage
Gets or sets the number of data records per page in the source file. Required.

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

### Microsoft.Crm.Sdk.Messages.GetDistinctValuesImportFileResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetDistinctValuesImportFileResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GetDistinctValuesImportFileResponse)
## NOTES

## RELATED LINKS
