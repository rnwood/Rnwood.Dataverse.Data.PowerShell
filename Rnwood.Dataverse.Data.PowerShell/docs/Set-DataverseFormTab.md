---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseFormTab

## SYNOPSIS
Creates or updates a tab on a Dataverse form with support for column layouts.

## SYNTAX

### Update
```
Set-DataverseFormTab -FormId <Guid> -TabId <String> -Name <String> [-Label <String>] [-LanguageCode <Int32>]
 [-Expanded] [-Visible] [-ShowLabel] [-VerticalLayout] [-Layout <String>] [-Column1Width <Int32>]
 [-Column2Width <Int32>] [-Column3Width <Int32>] [-PassThru] [-Publish] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Create
```
Set-DataverseFormTab -FormId <Guid> -Name <String> [-Label <String>] [-LanguageCode <Int32>] [-Expanded]
 [-Visible] [-ShowLabel] [-VerticalLayout] [-Layout <String>] [-Column1Width <Int32>] [-Column2Width <Int32>]
 [-Column3Width <Int32>] [-Index <Int32>] [-InsertBefore <String>] [-InsertAfter <String>] [-PassThru]
 [-Publish] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseFormTab cmdlet creates new tabs or updates existing ones on Dataverse forms. It supports comprehensive column layout management, allowing you to configure one, two, or three column layouts with custom width percentages.

The cmdlet can create new tabs or update existing ones by name or ID. When updating column layouts, existing sections are automatically redistributed across the new column structure.

## EXAMPLES

### Example 1: Create a simple tab
```powershell
PS C:\> Set-DataverseFormTab -Connection $c -FormId $formId -Name "Details" -Label "Details Tab"
```

Creates a new tab with default single-column layout.

### Example 2: Create a two-column tab
```powershell
PS C:\> Set-DataverseFormTab -Connection $c -FormId $formId -Name "ContactInfo" -Label "Contact Information" `
    -Layout TwoColumns -Column1Width 60 -Column2Width 40
```

Creates a tab with two columns: 60% and 40% width.

### Example 3: Create a three-column tab
```powershell
PS C:\> Set-DataverseFormTab -Connection $c -FormId $formId -Name "Overview" -Label "Overview" `
    -Layout ThreeColumns -Column1Width 40 -Column2Width 30 -Column3Width 30
```

Creates a tab with three columns with custom widths.

### Example 4: Update existing tab layout
```powershell
PS C:\> Set-DataverseFormTab -Connection $c -FormId $formId -Name "general" `
    -Layout TwoColumns -Column1Width "50%" -Column2Width "50%"
```

Updates an existing tab to use a two-column layout with equal widths.

### Example 5: Create tab with positioning
```powershell
PS C:\> Set-DataverseFormTab -Connection $c -FormId $formId -Name "Summary" -Label "Summary" `
    -InsertBefore "general" -Layout OneColumn
```

Creates a new tab and positions it before the "general" tab.

### Example 6: Convert three-column to two-column
```powershell
PS C:\> $tabId = Get-DataverseFormTab -Connection $c -FormId $formId -TabName "details" | Select-Object -ExpandProperty Id
PS C:\> Set-DataverseFormTab -Connection $c -FormId $formId -TabId $tabId -Name "details" `
    -Layout TwoColumns -Column1Width "70%" -Column2Width "30%"
```

Updates an existing tab to change from any layout to a two-column layout, redistributing sections automatically.

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

### -Expanded
Whether the tab is expanded by default.

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

### -FormId
ID of the form to create or update the tab on.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Index
Zero-based index position to insert the tab. Only used when creating new tabs.

```yaml
Type: Int32
Parameter Sets: Create
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InsertAfter
Name or ID of the tab after which to insert this tab. Only used when creating new tabs.

```yaml
Type: String
Parameter Sets: Create
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InsertBefore
Name or ID of the tab before which to insert this tab. Only used when creating new tabs.

```yaml
Type: String
Parameter Sets: Create
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Label
Display label text for the tab.

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

### -LanguageCode
Language code for the label (default: 1033 for English).

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 1033
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Logical name of the tab. Required for all operations.

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

### -PassThru
If specified, returns the tab ID.

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

### -Publish
Publishes the form after modifying the tab.

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

### -ShowLabel
Whether to show the tab label.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: True
Accept pipeline input: False
Accept wildcard characters: False
```

### -TabId
ID of the existing tab to update. Required when using the Update parameter set.

```yaml
Type: String
Parameter Sets: Update
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -VerticalLayout
Whether to use vertical layout for the tab.

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

### -Visible
Whether the tab is visible.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: True
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

### -Column1Width
Width of the first column as percentage (0-100). Used with TwoColumns or ThreeColumns layout.

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

### -Column2Width
Width of the second column as percentage (0-100). Used with TwoColumns or ThreeColumns layout.

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

### -Column3Width
Width of the third column as percentage (0-100). Used with ThreeColumns layout.

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

### -Layout
Column layout for the tab. Valid values: OneColumn, TwoColumns, ThreeColumns.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
## OUTPUTS

### System.String
## NOTES

**Column Layouts:**
- **OneColumn**: Single column layout (default width 100%)
- **TwoColumns**: Two-column layout (default widths 50%/50%)
- **ThreeColumns**: Three-column layout (default widths 33%/33%/34%)

**Column Width Guidelines:**
- Widths must be specified as percentages (e.g., "50%", "33%")
- Total width should typically sum to 100%
- Default widths are provided if not specified

**Section Redistribution:**
- When changing layouts, existing sections are automatically redistributed
- Sections are distributed evenly across the new column structure
- Section content and configuration is preserved

**Form Publishing:**
- Use -Publish to immediately publish changes
- Publishing is required for changes to appear in the UI
- Can also publish separately using Publish-DataverseEntity

**Update vs Create:**
- Tabs are identified by name for upsert behavior
- Specify -TabId for explicit updates to existing tabs
- Use positioning parameters only when creating new tabs

## RELATED LINKS

[Get-DataverseFormTab](Get-DataverseFormTab.md)

[Remove-DataverseFormTab](Remove-DataverseFormTab.md)

[Get-DataverseFormSection](Get-DataverseFormSection.md)

[Set-DataverseFormSection](Set-DataverseFormSection.md)
