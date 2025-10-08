---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseProvisionLanguageAsync

## SYNOPSIS
Contains the data that is needed to provision a new language using a background job.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ProvisionLanguageAsyncRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ProvisionLanguageAsyncRequest)

## SYNTAX

```
Invoke-DataverseProvisionLanguageAsync -Connection <ServiceClient> -Language <Int32>
```

## DESCRIPTION
Contains the data that is needed to provision a new language using a background job.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseProvisionLanguageAsync -Connection <ServiceClient> -Language <Int32>
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
Gets or sets the language to provision.

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

### Microsoft.Crm.Sdk.Messages.ProvisionLanguageAsyncResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ProvisionLanguageAsyncResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ProvisionLanguageAsyncResponse)
## NOTES

## RELATED LINKS
