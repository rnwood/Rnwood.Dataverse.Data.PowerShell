---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseStageAndUpgrade

## SYNOPSIS
Contains the data to import a solution, stage it for upgrade, and apply the upgrade as the default (when applicable).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.StageAndUpgradeRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.StageAndUpgradeRequest)

## SYNTAX

```
Invoke-DataverseStageAndUpgrade -Connection <ServiceClient> -OverwriteUnmanagedCustomizations <Boolean> -PublishWorkflows <Boolean> -CustomizationFile <Byte[]> -InFile <String> -ImportJobId <Guid> -ConvertToManaged <Boolean> -SkipProductUpdateDependencies <Boolean> -SkipQueueRibbonJob <Boolean> -AsyncRibbonProcessing <Boolean> -ComponentParameters <EntityCollection> -SolutionParameters <SolutionParameters> -LayerDesiredOrder <LayerDesiredOrder>
```

## DESCRIPTION
Contains the data to import a solution, stage it for upgrade, and apply the upgrade as the default (when applicable).

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseStageAndUpgrade -Connection <ServiceClient> -OverwriteUnmanagedCustomizations <Boolean> -PublishWorkflows <Boolean> -CustomizationFile <Byte[]> -InFile <String> -ImportJobId <Guid> -ConvertToManaged <Boolean> -SkipProductUpdateDependencies <Boolean> -SkipQueueRibbonJob <Boolean> -AsyncRibbonProcessing <Boolean> -ComponentParameters <EntityCollection> -SolutionParameters <SolutionParameters> -LayerDesiredOrder <LayerDesiredOrder>
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

### -OverwriteUnmanagedCustomizations
Gets or sets whether any unmanaged customizations that have been applied over existing managed solution components should be overwritten. Required.

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

### -PublishWorkflows
Gets or sets whether any processes (workflows) included in the solution should be activated after they are imported. Required.

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

### -CustomizationFile
Gets or sets the compressed solutions file to import. Required.

```yaml
Type: Byte[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InFile
Gets or sets the path to a file containing the data to upload.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ImportJobId
Gets or sets the ID of the import job that will be created to perform this import. Required.

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

### -ConvertToManaged
Gets or sets whether to convert any matching unmanaged customizations into your managed solution. Obsolete.

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

### -SkipProductUpdateDependencies
Gets or sets whether enforcement of dependencies related to product updates should be skipped.

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

### -SkipQueueRibbonJob
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

### -AsyncRibbonProcessing
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

### -ComponentParameters
Gets or sets the list of entities to overwrite values from the solution.

```yaml
Type: EntityCollection
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SolutionParameters
Gets or sets additional solution parameters.

```yaml
Type: SolutionParameters
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LayerDesiredOrder
For internal use only.

```yaml
Type: LayerDesiredOrder
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

### Microsoft.Crm.Sdk.Messages.StageAndUpgradeResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.StageAndUpgradeResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.StageAndUpgradeResponse)
## NOTES

## RELATED LINKS
