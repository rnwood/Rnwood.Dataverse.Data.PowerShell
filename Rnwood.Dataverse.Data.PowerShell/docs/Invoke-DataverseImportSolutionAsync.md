---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseImportSolutionAsync

## SYNOPSIS
Contains the data that is needed to import a solution using an asynchronous job.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImportSolutionAsyncRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ImportSolutionAsyncRequest)

## SYNTAX

```
Invoke-DataverseImportSolutionAsync -Connection <ServiceClient> [-OverwriteUnmanagedCustomizations <Boolean>]
 [-PublishWorkflows <Boolean>] [-CustomizationFile <Byte[]>] [-ImportJobId <Guid>]
 [-ConvertToManaged <Boolean>] [-SkipProductUpdateDependencies <Boolean>] [-HoldingSolution <Boolean>]
 [-SkipQueueRibbonJob <Boolean>] [-LayerDesiredOrder <LayerDesiredOrder>] [-AsyncRibbonProcessing <Boolean>]
 [-ComponentParameters <EntityCollection>] [-SolutionParameters <SolutionParameters>]
 [-IsTemplateMode <Boolean>] [-TemplateSuffix <String>] [-TemplateDisplayNamePrefix <String>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to import a solution using an asynchronous job.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseImportSolutionAsync -Connection <ServiceClient> -OverwriteUnmanagedCustomizations <Boolean> -PublishWorkflows <Boolean> -CustomizationFile <Byte[]> -ImportJobId <Guid> -ConvertToManaged <Boolean> -SkipProductUpdateDependencies <Boolean> -HoldingSolution <Boolean> -SkipQueueRibbonJob <Boolean> -LayerDesiredOrder <LayerDesiredOrder> -AsyncRibbonProcessing <Boolean> -ComponentParameters <EntityCollection> -SolutionParameters <SolutionParameters> -IsTemplateMode <Boolean> -TemplateSuffix <String> -TemplateDisplayNamePrefix <String>
```

## PARAMETERS

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

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

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

### -ConvertToManaged
Obsolete. The system will convert unmanaged solution components to managed when you import a managed solution.

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

### -HoldingSolution
Gets or sets whether to import the solution as a holding solution staged for upgrade.

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

### -ImportJobId
The ID of the Import Job.

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

### -IsTemplateMode
Internal use only.

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

### -SolutionParameters
Gets or sets additional parameters for the solution.

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

### -TemplateDisplayNamePrefix
Gets or sets the TemplateDisplayNamePrefix for the request.

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

### -TemplateSuffix
Internal use only.

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
Default value: False
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
