---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataversePickFromQueue

## SYNOPSIS
Contains the data that is needed to assign a queue item to a user and optionally remove the queue item from the queue.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.PickFromQueueRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.PickFromQueueRequest)

## SYNTAX

```
Invoke-DataversePickFromQueue -Connection <ServiceClient> -QueueItemId <Guid> -WorkerId <Guid> -RemoveQueueItem <Boolean>
```

## DESCRIPTION
Contains the data that is needed to assign a queue item to a user and optionally remove the queue item from the queue.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataversePickFromQueue -Connection <ServiceClient> -QueueItemId <Guid> -WorkerId <Guid> -RemoveQueueItem <Boolean>
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
Gets or sets the

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

### -WorkerId
Gets or sets the user to assign the queue item to. Required.

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

### -RemoveQueueItem
Gets or sets whether the queue item should be removed from the queue.

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

### Microsoft.Crm.Sdk.Messages.PickFromQueueResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.PickFromQueueResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.PickFromQueueResponse)
## NOTES

## RELATED LINKS
