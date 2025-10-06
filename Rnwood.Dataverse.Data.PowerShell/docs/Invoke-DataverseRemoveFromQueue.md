---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRemoveFromQueue

## SYNOPSIS
Contains the data that is needed to remove a queue item from a queue.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveFromQueueRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RemoveFromQueueRequest)

## SYNTAX

```
Invoke-DataverseRemoveFromQueue -Connection <ServiceClient> -QueueItemId <Guid>
```

## DESCRIPTION
Contains the data that is needed to remove a queue item from a queue.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRemoveFromQueue -Connection <ServiceClient> -QueueItemId <Guid>
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

### -QueueItemId
Gets or sets the ID of the queue item to remove from the queue. Required.The property corresponds to the attribute, which is the primary key for the entity.

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

### Microsoft.Crm.Sdk.Messages.RemoveFromQueueResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveFromQueueResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RemoveFromQueueResponse)
## NOTES

## RELATED LINKS
