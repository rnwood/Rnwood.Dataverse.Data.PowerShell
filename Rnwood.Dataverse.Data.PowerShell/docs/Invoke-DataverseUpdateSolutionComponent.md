---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseUpdateSolutionComponent

## SYNOPSIS
Contains the data that is needed to update a component in an unmanaged solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UpdateSolutionComponentRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.UpdateSolutionComponentRequest)

## SYNTAX

```
Invoke-DataverseUpdateSolutionComponent -Connection <ServiceClient> -ComponentId <Guid> -ComponentType <Int32> -SolutionUniqueName <String> -IncludedComponentSettingsValues <String[]>
```

## DESCRIPTION
Contains the data that is needed to update a component in an unmanaged solution.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseUpdateSolutionComponent -Connection <ServiceClient> -ComponentId <Guid> -ComponentType <Int32> -SolutionUniqueName <String> -IncludedComponentSettingsValues <String[]>
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
Gets or sets the unique identifier of the component to update.

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
Gets or sets the type of component to be updated.

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

### Microsoft.Crm.Sdk.Messages.UpdateSolutionComponentResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UpdateSolutionComponentResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.UpdateSolutionComponentResponse)
## NOTES

## RELATED LINKS
