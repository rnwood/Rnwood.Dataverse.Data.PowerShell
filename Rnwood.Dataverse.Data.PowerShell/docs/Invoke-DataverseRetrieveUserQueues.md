---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveUserQueues

## SYNOPSIS
Contains the data needed to retrieve all private queues of a specified user and optionally all public queues.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUserQueuesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveUserQueuesRequest)

## SYNTAX

```
Invoke-DataverseRetrieveUserQueues -Connection <ServiceClient> -UserId <Guid> -IncludePublic <Boolean>
```

## DESCRIPTION
Contains the data needed to retrieve all private queues of a specified user and optionally all public queues.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveUserQueues -Connection <ServiceClient> -UserId <Guid> -IncludePublic <Boolean>
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

### -UserId
Gets or sets the Microsoft Dynamics 365 system user ID of the client.

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

### -IncludePublic
Gets or sets whether the response should include public queues.

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

### Microsoft.Crm.Sdk.Messages.RetrieveUserQueuesResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUserQueuesResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveUserQueuesResponse)
## NOTES

## RELATED LINKS
