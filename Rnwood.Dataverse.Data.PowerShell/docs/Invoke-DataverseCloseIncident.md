---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCloseIncident

## SYNOPSIS
Contains the data that is needed to close an incident (case).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CloseIncidentRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CloseIncidentRequest)

## SYNTAX

```
Invoke-DataverseCloseIncident -Connection <ServiceClient> -IncidentResolution <PSObject> -IncidentResolutionTableName <String> -IncidentResolutionIgnoreProperties <String[]> -IncidentResolutionLookupColumns <Hashtable> -Status <OptionSetValue>
```

## DESCRIPTION
Contains the data that is needed to close an incident (case).

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCloseIncident -Connection <ServiceClient> -IncidentResolution <PSObject> -IncidentResolutionTableName <String> -IncidentResolutionIgnoreProperties <String[]> -IncidentResolutionLookupColumns <Hashtable> -Status <OptionSetValue>
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

### -IncidentResolution
Gets or sets the incident resolution (case resolution) that is associated with the incident (case) to be closed. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncidentResolutionTableName
Gets or sets the incident resolution (case resolution) that is associated with the incident (case) to be closed. Required. The logical name of the table/entity type for the IncidentResolution parameter.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncidentResolutionIgnoreProperties
Gets or sets the incident resolution (case resolution) that is associated with the incident (case) to be closed. Required. Properties to ignore when converting IncidentResolution PSObject to Entity.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncidentResolutionLookupColumns
Gets or sets the incident resolution (case resolution) that is associated with the incident (case) to be closed. Required. Hashtable specifying lookup columns for entity reference conversions in IncidentResolution.

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Status
Gets or sets a status of the incident. Required.

```yaml
Type: OptionSetValue
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

### Microsoft.Crm.Sdk.Messages.CloseIncidentResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CloseIncidentResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CloseIncidentResponse)
## NOTES

## RELATED LINKS
