---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseExportSolution

## SYNOPSIS
Contains the data needed to export a solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExportSolutionRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ExportSolutionRequest)

## SYNTAX

```
Invoke-DataverseExportSolution -Connection <ServiceClient> -SolutionName <String> -Managed <Boolean> -TargetVersion <String> -ExportAutoNumberingSettings <Boolean> -ExportCalendarSettings <Boolean> -ExportCustomizationSettings <Boolean> -ExportEmailTrackingSettings <Boolean> -ExportGeneralSettings <Boolean> -ExportMarketingSettings <Boolean> -ExportOutlookSynchronizationSettings <Boolean> -ExportRelationshipRoles <Boolean> -ExportIsvConfig <Boolean> -ExportSales <Boolean> -ExportExternalApplications <Boolean> -ExportComponentsParams <ExportComponentsParams>
```

## DESCRIPTION
Contains the data needed to export a solution.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseExportSolution -Connection <ServiceClient> -SolutionName <String> -Managed <Boolean> -TargetVersion <String> -ExportAutoNumberingSettings <Boolean> -ExportCalendarSettings <Boolean> -ExportCustomizationSettings <Boolean> -ExportEmailTrackingSettings <Boolean> -ExportGeneralSettings <Boolean> -ExportMarketingSettings <Boolean> -ExportOutlookSynchronizationSettings <Boolean> -ExportRelationshipRoles <Boolean> -ExportIsvConfig <Boolean> -ExportSales <Boolean> -ExportExternalApplications <Boolean> -ExportComponentsParams <ExportComponentsParams>
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

### -SolutionName
Gets or sets the unique name of the solution to be exported. Required.

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

### -Managed
Gets or sets whether the solution should be exported as a managed solution. Required.

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

### -TargetVersion
Get or set a value indicating the version that the exported solution will support.

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

### -ExportAutoNumberingSettings
Gets or sets whether auto numbering settings should be included in the solution being exported. Optional.

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

### -ExportCalendarSettings
Gets or sets whether calendar settings should be included in the solution being exported. Optional.

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

### -ExportCustomizationSettings
Gets or sets whether customization settings should be included in the solution being exported. Optional.

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

### -ExportEmailTrackingSettings
Gets or sets whether email tracking settings should be included in the solution being exported. Optional.

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

### -ExportGeneralSettings
Gets or sets whether general settings should be included in the solution being exported. Optional.

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

### -ExportMarketingSettings
Gets or sets whether marketing settings should be included in the solution being exported. Optional.

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

### -ExportOutlookSynchronizationSettings
Gets or sets whether outlook synchronization settings should be included in the solution being exported. Optional.

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

### -ExportRelationshipRoles
Gets or sets whether relationship role settings should be included in the solution being exported. Optional.

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

### -ExportIsvConfig
Gets or sets whether ISV.Config settings should be included in the solution being exported. Optional.

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

### -ExportSales
Gets or sets whether sales settings should be included in the solution being exported. Optional.

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

### -ExportExternalApplications
For internal use only.

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

### -ExportComponentsParams
Gets or sets whether solution component parameters should be included in the solution being exported. Optional.

```yaml
Type: ExportComponentsParams
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

### Microsoft.Crm.Sdk.Messages.ExportSolutionResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExportSolutionResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ExportSolutionResponse)
## NOTES

## RELATED LINKS
