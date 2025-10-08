---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseSetReportRelated

## SYNOPSIS
Contains the data needed to link an instance of a report entity to related entities.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SetReportRelatedRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SetReportRelatedRequest)

## SYNTAX

```
Invoke-DataverseSetReportRelated -Connection <ServiceClient> -ReportId <Guid> -Entities <Int32> -Categories <Int32> -Visibility <Int32>
```

## DESCRIPTION
Contains the data needed to link an instance of a report entity to related entities.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseSetReportRelated -Connection <ServiceClient> -ReportId <Guid> -Entities <Int32> -Categories <Int32> -Visibility <Int32>
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

### -ReportId
Gets or sets the ID of the report. Required.

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

### -Entities
Gets the collection of entities.

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

### -Categories
Gets or sets an array of report category codes. Required.

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

### -Visibility
Gets or sets an array of report visibility codes. Required.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.SetReportRelatedResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SetReportRelatedResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SetReportRelatedResponse)
## NOTES

## RELATED LINKS
