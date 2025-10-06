---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseQuerySchedule

## SYNOPSIS
Contains the data that is needed to search the specified resource for an available time block that matches the specified parameters.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.QueryScheduleRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.QueryScheduleRequest)

## SYNTAX

```
Invoke-DataverseQuerySchedule -Connection <ServiceClient> -ResourceId <Guid> -Start <DateTime> -End <DateTime> -TimeCodes <TimeCode[]>
```

## DESCRIPTION
Contains the data that is needed to search the specified resource for an available time block that matches the specified parameters.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseQuerySchedule -Connection <ServiceClient> -ResourceId <Guid> -Start <DateTime> -End <DateTime> -TimeCodes <TimeCode[]>
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

### -ResourceId
Gets or sets the ID of the resource.

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

### -Start
Gets or sets the start of the period to expand.

```yaml
Type: DateTime
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -End
Gets or sets the end of the time period to expand.

```yaml
Type: DateTime
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TimeCodes
Gets or sets the time codes to look for: Available, Busy, Unavailable, or Filter, which correspond to the resource IDs. Required.

```yaml
Type: TimeCode[]
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

### Microsoft.Crm.Sdk.Messages.QueryScheduleResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.QueryScheduleResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.QueryScheduleResponse)
## NOTES

## RELATED LINKS
