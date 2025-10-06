---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveProvisionedLanguagePackVersion

## SYNOPSIS
Contains the data that is needed to retrieve the version of a provisioned language pack.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagePackVersionRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagePackVersionRequest)

## SYNTAX

```
Invoke-DataverseRetrieveProvisionedLanguagePackVersion -Connection <ServiceClient> -Language <Int32>
```

## DESCRIPTION
Contains the data that is needed to retrieve the version of a provisioned language pack.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveProvisionedLanguagePackVersion -Connection <ServiceClient> -Language <Int32>
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

### -Language
Gets or sets the Locale Id for the language. Required.

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

### Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagePackVersionResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagePackVersionResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagePackVersionResponse)
## NOTES

## RELATED LINKS
