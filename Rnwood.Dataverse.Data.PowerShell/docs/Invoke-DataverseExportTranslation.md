---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseExportTranslation

## SYNOPSIS
Contains the data that is needed to export all translations for a specific solution to a compressed file.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExportTranslationRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ExportTranslationRequest)

## SYNTAX

```
Invoke-DataverseExportTranslation -Connection <ServiceClient> -SolutionName <String>
```

## DESCRIPTION
Contains the data that is needed to export all translations for a specific solution to a compressed file.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseExportTranslation -Connection <ServiceClient> -SolutionName <String>
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

### -SolutionName
Gets or sets the unique name for the unmanaged solution to export translations for. Required.

```yaml
Type: String
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

### Microsoft.Crm.Sdk.Messages.ExportTranslationResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExportTranslationResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ExportTranslationResponse)
## NOTES

## RELATED LINKS
