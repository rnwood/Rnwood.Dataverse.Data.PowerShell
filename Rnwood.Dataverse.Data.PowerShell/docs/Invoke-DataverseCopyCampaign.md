---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCopyCampaign

## SYNOPSIS
Contains the data that is needed to copy a campaign.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CopyCampaignRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CopyCampaignRequest)

## SYNTAX

```
Invoke-DataverseCopyCampaign -Connection <ServiceClient> -BaseCampaign <Guid> -SaveAsTemplate <Boolean>
```

## DESCRIPTION
Contains the data that is needed to copy a campaign.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCopyCampaign -Connection <ServiceClient> -BaseCampaign <Guid> -SaveAsTemplate <Boolean>
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

### -BaseCampaign
Gets or sets the ID of the base campaign to copy from. Required.

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

### -SaveAsTemplate
Gets or sets a value that indicates whether to save the campaign as a template. Required.

```yaml
Type: Boolean
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

### Microsoft.Crm.Sdk.Messages.CopyCampaignResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CopyCampaignResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CopyCampaignResponse)
## NOTES

## RELATED LINKS
