---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseImportTranslation

## SYNOPSIS
Contains the data that is needed to import translations from a compressed file.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImportTranslationRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ImportTranslationRequest)

## SYNTAX

```
Invoke-DataverseImportTranslation -Connection <ServiceClient> -TranslationFile <Byte[]> -InFile <String> -ImportJobId <Guid>
```

## DESCRIPTION
Contains the data that is needed to import translations from a compressed file.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseImportTranslation -Connection <ServiceClient> -TranslationFile <Byte[]> -InFile <String> -ImportJobId <Guid>
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

### -TranslationFile
Gets or sets the compressed translations file. Required.

```yaml
Type: Byte[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InFile
Gets or sets the path to a file containing the data to upload.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ImportJobId
The ID of the Import Job.

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

### Microsoft.Crm.Sdk.Messages.ImportTranslationResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImportTranslationResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ImportTranslationResponse)
## NOTES

## RELATED LINKS
