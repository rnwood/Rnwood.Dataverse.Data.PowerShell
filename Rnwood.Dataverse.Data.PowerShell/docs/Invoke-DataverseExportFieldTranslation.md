---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseExportFieldTranslation

## SYNOPSIS
Contains the data that is needed to export localizable fields values to a compressed file.For the Web API use ExportFieldTranslation Function.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExportFieldTranslationRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ExportFieldTranslationRequest)

## SYNTAX

```
Invoke-DataverseExportFieldTranslation -Connection <ServiceClient>
```

## DESCRIPTION
Contains the data that is needed to export localizable fields values to a compressed file.For the Web API use ExportFieldTranslation Function.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseExportFieldTranslation -Connection <ServiceClient>
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

### Microsoft.Crm.Sdk.Messages.ExportFieldTranslationResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExportFieldTranslationResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ExportFieldTranslationResponse)
## NOTES

## RELATED LINKS
