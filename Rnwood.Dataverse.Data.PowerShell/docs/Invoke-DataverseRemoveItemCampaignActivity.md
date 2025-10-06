---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRemoveItemCampaignActivity

## SYNOPSIS
Contains the data that is needed to remove an item from a campaign activity.This message does not have a corresponding Web API action or function in Microsoft Dynamics 365 (online &amp; on-premises). More information: Missing functions and actions for some organization service messages.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveItemCampaignActivityRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RemoveItemCampaignActivityRequest)

## SYNTAX

```
Invoke-DataverseRemoveItemCampaignActivity -Connection <ServiceClient> -CampaignActivityId <Guid> -ItemId <Guid>
```

## DESCRIPTION
Contains the data that is needed to remove an item from a campaign activity.This message does not have a corresponding Web API action or function in Microsoft Dynamics 365 (online &amp; on-premises). More information: Missing functions and actions for some organization service messages.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRemoveItemCampaignActivity -Connection <ServiceClient> -CampaignActivityId <Guid> -ItemId <Guid>
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

### -CampaignActivityId
Gets or sets the ID of the campaign activity. Required.

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

### -ItemId
Gets or sets the ID of the item to be added to the campaign activity. Required.

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

### Microsoft.Crm.Sdk.Messages.RemoveItemCampaignActivityResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveItemCampaignActivityResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RemoveItemCampaignActivityResponse)
## NOTES

## RELATED LINKS
