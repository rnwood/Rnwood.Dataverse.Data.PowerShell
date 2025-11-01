---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseOptionSetMetadata

## SYNOPSIS
Creates or updates a global option set in Dataverse.

## SYNTAX

```
Set-DataverseOptionSetMetadata [-Name] <String> [-DisplayName <String>] [-Description <String>]
 -Options <Hashtable[]> [-NoRemoveMissingOptions] [-PassThru] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet creates or updates a global option set in Dataverse. It can add new options, update existing options, and optionally remove options that are not provided.

The cmdlet supports:
- Creating new global option sets
- Updating display name and description of existing option sets
- Adding new options to existing option sets
- Updating labels, colors, and descriptions of existing options
- Removing options that are not provided (unless -NoRemoveMissingOptions is specified)

Options are specified as an array of hashtables, each containing:
- Value: The numeric value of the option (required for updates, optional for new options)
- Label: The display label of the option (required)
- Color: Optional color for the option
- Description: Optional description for the option

## EXAMPLES

### Example 1: Create a new global option set
```powershell
PS C:\> $options = @(
    @{ Value = 1; Label = "Option 1" }
    @{ Value = 2; Label = "Option 2"; Color = "#FF0000" }
    @{ Value = 3; Label = "Option 3"; Description = "Third option" }
)

PS C:\> Set-DataverseOptionSetMetadata -Connection $c -Name "new_optionset" -DisplayName "New Option Set" -Options $options
```

Creates a new global option set with three options.

### Example 2: Update an existing option set
```powershell
PS C:\> $options = @(
    @{ Value = 1; Label = "Updated Option 1" }
    @{ Value = 2; Label = "Option 2" }
    @{ Value = 4; Label = "New Option 4" }
)

PS C:\> Set-DataverseOptionSetMetadata -Connection $c -Name "existing_optionset" -Options $options
```

Updates an existing option set by changing the label of option 1, keeping option 2 unchanged, and adding a new option 4. Option 3 (if it existed) would be removed unless -NoRemoveMissingOptions is specified.

### Example 3: Update option set metadata without changing options
```powershell
PS C:\> Set-DataverseOptionSetMetadata -Connection $c -Name "my_optionset" -DisplayName "Updated Display Name" -Description "Updated description"
```

Updates only the display name and description of an existing option set without modifying any options.

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
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.

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

### -Description
The description of the option set.

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

### -DisplayName
The display name of the option set.

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

### -Name
The name of the global option set.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoRemoveMissingOptions
Do not remove existing options that are not provided.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Options
Array of hashtables defining options. Each hashtable should contain:
- Value: Numeric value (required for updates)
- Label: Display label (required)
- Color: Optional color
- Description: Optional description

```yaml
Type: Hashtable[]
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Return the created/updated option set metadata.

```yaml
Type: SwitchParameter
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

### System.Management.Automation.PSObject
## NOTES

This cmdlet provides programmatic access to Dataverse metadata. For comprehensive documentation and examples, see the metadata concept guide at docs/core-concepts/metadata.md

## RELATED LINKS
