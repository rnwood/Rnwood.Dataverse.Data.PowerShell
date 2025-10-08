---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseAddSolutionComponent

## SYNOPSIS
Contains the data that is needed to add a solution component to an unmanaged solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddSolutionComponentRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.AddSolutionComponentRequest)

## SYNTAX

```
Invoke-DataverseAddSolutionComponent -Connection <ServiceClient> -ComponentId <Guid> -ComponentType <Int32> -SolutionUniqueName <String> -AddRequiredComponents <Boolean> -DoNotIncludeSubcomponents <Boolean> -IncludedComponentSettingsValues <String[]>
```

## DESCRIPTION
Contains the data that is needed to add a solution component to an unmanaged solution.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseAddSolutionComponent -Connection <ServiceClient> -ComponentId <Guid> -ComponentType <Int32> -SolutionUniqueName <String> -AddRequiredComponents <Boolean> -DoNotIncludeSubcomponents <Boolean> -IncludedComponentSettingsValues <String[]>
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

### -ComponentId
Gets or sets the ID of the solution component. Required.

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

### -ComponentType
Gets or sets the value that represents the solution component that you are adding. Required.

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

### -SolutionUniqueName
Gets or sets the name of the unmanaged solution to which you want to add this column. Optional.

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

### -AddRequiredComponents
Gets or sets a value that indicates whether other solution components that are required by the solution component that you are adding should also be added to the unmanaged solution. Required.

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

### -DoNotIncludeSubcomponents
Indicates whether the subcomponents should be included.

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

### -IncludedComponentSettingsValues
Gets or sets a value that specifies if the component is added to the solution with its metadata.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.AddSolutionComponentResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddSolutionComponentResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.AddSolutionComponentResponse)
## NOTES

## RELATED LINKS
