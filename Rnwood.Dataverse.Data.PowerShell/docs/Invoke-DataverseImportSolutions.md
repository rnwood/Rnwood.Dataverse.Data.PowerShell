---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseImportSolutions

## SYNOPSIS
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImportSolutionsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ImportSolutionsRequest?view=dataverse-sdk-latest)

## SYNTAX

### Default (Default)
```
Invoke-DataverseImportSolutions [-OverwriteUnmanagedCustomizations <Boolean>] [-PublishWorkflows <Boolean>]
 [-CustomizationFiles <Byte[]>] [-ImportJobId <Guid>] [-ConvertToManaged <Boolean>]
 [-SkipProductUpdateDependencies <Boolean>] [-HoldingSolution <Boolean>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### FromFile
```
Invoke-DataverseImportSolutions [-OverwriteUnmanagedCustomizations <Boolean>] [-PublishWorkflows <Boolean>]
 -InFile <String> [-ImportJobId <Guid>] [-ConvertToManaged <Boolean>]
 [-SkipProductUpdateDependencies <Boolean>] [-HoldingSolution <Boolean>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
For internal use only.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseImportSolutions -Connection <ServiceClient> -OverwriteUnmanagedCustomizations <Boolean> -PublishWorkflows <Boolean> -CustomizationFiles <Byte[]> -InFile <String> -ImportJobId <Guid> -ConvertToManaged <Boolean> -SkipProductUpdateDependencies <Boolean> -HoldingSolution <Boolean>
```

## PARAMETERS

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

### -CustomizationFiles
Gets or sets the CustomizationFiles for the request.

```yaml
Type: Byte[]
Parameter Sets: Default
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

### -InFile
Gets or sets the path to a file containing the data to upload.

```yaml
Type: String
Parameter Sets: FromFile
Aliases:

Required: True
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

