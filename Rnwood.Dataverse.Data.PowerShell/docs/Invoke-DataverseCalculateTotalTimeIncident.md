---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCalculateTotalTimeIncident

## SYNOPSIS
Contains the data that is needed to calculate the total time, in minutes, that you used while you worked on an incident (case).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CalculateTotalTimeIncidentRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CalculateTotalTimeIncidentRequest)

## SYNTAX

```
Invoke-DataverseCalculateTotalTimeIncident -Connection <ServiceClient> -IncidentId <Guid>
```

## DESCRIPTION
Contains the data that is needed to calculate the total time, in minutes, that you used while you worked on an incident (case).

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCalculateTotalTimeIncident -Connection <ServiceClient> -IncidentId <Guid>
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

### -IncidentId
Gets or sets the ID of the incident (case). Required.

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

### Microsoft.Crm.Sdk.Messages.CalculateTotalTimeIncidentResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CalculateTotalTimeIncidentResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CalculateTotalTimeIncidentResponse)
## NOTES

## RELATED LINKS
