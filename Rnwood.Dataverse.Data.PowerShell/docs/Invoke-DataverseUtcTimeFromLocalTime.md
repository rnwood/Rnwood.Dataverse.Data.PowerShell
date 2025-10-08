---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseUtcTimeFromLocalTime

## SYNOPSIS
Contains the data that is needed to retrieve the Coordinated Universal Time (UTC) for the specified local time.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UtcTimeFromLocalTimeRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.UtcTimeFromLocalTimeRequest)

## SYNTAX

```
Invoke-DataverseUtcTimeFromLocalTime -Connection <ServiceClient> -TimeZoneCode <Int32> -LocalTime <DateTime>
```

## DESCRIPTION
Contains the data that is needed to retrieve the Coordinated Universal Time (UTC) for the specified local time.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseUtcTimeFromLocalTime -Connection <ServiceClient> -TimeZoneCode <Int32> -LocalTime <DateTime>
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

### -TimeZoneCode
Gets or sets the time zone code. Required.

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

### -LocalTime
Gets or sets the local time. Required.

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

### Microsoft.Crm.Sdk.Messages.UtcTimeFromLocalTimeResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UtcTimeFromLocalTimeResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.UtcTimeFromLocalTimeResponse)
## NOTES

## RELATED LINKS
