---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRemoveItemCampaign

## SYNOPSIS
Contains the data that is needed to remove an item from a campaign.This message does not have a corresponding Web API action or function in Microsoft Dynamics 365 (online &amp; on-premises). More information: Missing functions and actions for some organization service messages.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveItemCampaignRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RemoveItemCampaignRequest)

## SYNTAX

```
Invoke-DataverseRemoveItemCampaign -Connection <ServiceClient> [-CampaignId <Guid>] [-EntityId <Guid>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to remove an item from a campaign.This message does not have a corresponding Web API action or function in Microsoft Dynamics 365 (online &amp; on-premises). More information: Missing functions and actions for some organization service messages.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRemoveItemCampaign -Connection <ServiceClient> -CampaignId <Guid> -EntityId <Guid>
```

## PARAMETERS

### -CampaignId
Gets or sets the ID of the campaign. Required.

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

### -EntityId
Gets the of the newly created table.

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

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
