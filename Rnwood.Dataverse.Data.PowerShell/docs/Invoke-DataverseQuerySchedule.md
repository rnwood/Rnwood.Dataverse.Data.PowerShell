---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseQuerySchedule

## SYNOPSIS
Contains the data that is needed to search the specified resource for an available time block that matches the specified parameters.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.QueryScheduleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.QueryScheduleRequest?view=dataverse-sdk-latest)

## SYNTAX

```
Invoke-DataverseQuerySchedule [-ResourceId <Guid>] [-Start <DateTime>] [-End <DateTime>]
 [-TimeCodes <TimeCode[]>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to search the specified resource for an available time block that matches the specified parameters.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseQuerySchedule -Connection <ServiceClient> -ResourceId <Guid> -Start <DateTime> -End <DateTime> -TimeCodes <TimeCode[]>
```

## PARAMETERS

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

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

### -TimeCodes
Gets or sets the time codes to look for: Available, Busy, Unavailable, or Filter, which correspond to the resource IDs. Required.

```yaml
Type: TimeCode[]
Parameter Sets: (All)
Aliases:
Accepted values: Available, Busy, Unavailable, Filter

Required: False
Position: Named
Default value: None
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS

