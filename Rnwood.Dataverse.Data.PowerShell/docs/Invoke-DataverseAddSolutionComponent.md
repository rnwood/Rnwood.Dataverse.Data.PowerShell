---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseAddSolutionComponent

## SYNOPSIS
Contains the data that is needed to add a solution component to an unmanaged solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddSolutionComponentRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AddSolutionComponentRequest?view=dataverse-sdk-latest)

## SYNTAX

```
Invoke-DataverseAddSolutionComponent [-ComponentId <Guid>] [-ComponentType <Int32>]
 [-SolutionUniqueName <String>] [-AddRequiredComponents <Boolean>] [-DoNotIncludeSubcomponents <Boolean>]
 [-IncludedComponentSettingsValues <String[]>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to add a solution component to an unmanaged solution.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseAddSolutionComponent -Connection <ServiceClient> -ComponentId <Guid> -ComponentType <Int32> -SolutionUniqueName <String> -AddRequiredComponents <Boolean> -DoNotIncludeSubcomponents <Boolean> -IncludedComponentSettingsValues <String[]>
```

## PARAMETERS

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
