---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseExpandCalendar

## SYNOPSIS
Contains the data that is needed to convert the calendar rules to an array of available time blocks for the specified period.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExpandCalendarRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ExpandCalendarRequest)

## SYNTAX

```
Invoke-DataverseExpandCalendar -Connection <ServiceClient> -CalendarId <Guid> -Start <DateTime> -End <DateTime>
```

## DESCRIPTION
Contains the data that is needed to convert the calendar rules to an array of available time blocks for the specified period.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseExpandCalendar -Connection <ServiceClient> -CalendarId <Guid> -Start <DateTime> -End <DateTime>
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

### -CalendarId
Gets or sets the ID of the calendar.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.ExpandCalendarResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExpandCalendarResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ExpandCalendarResponse)
## NOTES

## RELATED LINKS
