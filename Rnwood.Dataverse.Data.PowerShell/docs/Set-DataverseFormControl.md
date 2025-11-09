---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseFormControl

## SYNOPSIS
Creates or updates a control in a Dataverse form section.

## SYNTAX

### Update
```
Set-DataverseFormControl -FormId <Guid> -SectionName <String> -ControlId <String> -DataField <String>
 [-ControlType <String>] [-Label <String>] [-LanguageCode <Int32>] [-Disabled] [-Visible] [-Rows <Int32>]
 [-ColSpan <Int32>] [-RowSpan <Int32>] [-ShowLabel] [-IsRequired] [-Parameters <Hashtable>] [-PassThru]
 [-Publish] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### Create
```
Set-DataverseFormControl -FormId <Guid> -SectionName <String> -DataField <String> [-ControlType <String>]
 [-Label <String>] [-LanguageCode <Int32>] [-Disabled] [-Visible] [-Rows <Int32>] [-ColSpan <Int32>]
 [-RowSpan <Int32>] [-ShowLabel] [-IsRequired] [-Parameters <Hashtable>] [-Index <Int32>]
 [-InsertBefore <String>] [-InsertAfter <String>] [-PassThru] [-Publish] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### RawXml
```
Set-DataverseFormControl -FormId <Guid> -SectionName <String> -ControlXml <String> [-ControlType <String>]
 [-Label <String>] [-LanguageCode <Int32>] [-Disabled] [-Visible] [-Rows <Int32>] [-ColSpan <Int32>]
 [-RowSpan <Int32>] [-ShowLabel] [-IsRequired] [-Parameters <Hashtable>] [-Index <Int32>]
 [-InsertBefore <String>] [-InsertAfter <String>] [-PassThru] [-Publish] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseFormControl cmdlet creates new controls or updates existing controls in Dataverse form sections. Controls are the individual form elements (text boxes, dropdowns, lookups, etc.) that users interact with. The cmdlet supports all standard control types and allows raw XML for advanced scenarios.

You can specify control properties like data binding, positioning, visibility, labels, and behavior. When creating controls, you can specify their position using Index, InsertBefore, or InsertAfter parameters. For advanced scenarios, use the RawXml parameter set to provide complete control XML.

## EXAMPLES

### Example 1: Add a simple text field control
```powershell
PS C:\> $form = Get-DataverseForm -Connection $c -Entity 'contact' -Name 'Information'
PS C:\> Set-DataverseFormControl -Connection $c -FormId $form.Id -SectionName 'GeneralSection' `
    -DataField 'firstname' -Label 'First Name' -ShowLabel -Visible -PassThru
```

Adds a text field control for the firstname attribute with a custom label.

### Example 2: Create a lookup control with custom properties
```powershell
PS C:\> Set-DataverseFormControl -Connection $c -FormId $formId -SectionName 'ContactInfo' `
    -DataField 'parentcustomerid' -ControlType 'Lookup' `
    -Label 'Parent Account' -ColSpan 2 -IsRequired -PassThru
```

Creates a lookup control spanning 2 columns and marked as required.

### Example 3: Add a multiline text control
```powershell
PS C:\> Set-DataverseFormControl -Connection $c -FormId $formId -SectionName 'Details' `
    -DataField 'description' -ControlType 'Standard' `
    -Label 'Description' -Rows 4 -ColSpan 2 -PassThru
```

Creates a multiline text area with 4 rows spanning 2 columns.

### Example 4: Insert control at specific position
```powershell
PS C:\> Set-DataverseFormControl -Connection $c -FormId $formId -SectionName 'GeneralSection' `
    -DataField 'emailaddress1' -Label 'Primary Email' `
    -InsertAfter 'lastname' -ColSpan 2 -PassThru
```

Inserts an email control after the 'lastname' control.

### Example 5: Create a subgrid control with parameters
```powershell
PS C:\> $parameters = @{
    'targetEntityType' = 'opportunity'
    'viewId' = '12345678-1234-1234-1234-123456789012'
    'RelationshipName' = 'contact_customer_opportunity'
}
PS C:\> Set-DataverseFormControl -Connection $c -FormId $formId -SectionName 'RelatedData' `
    -DataField 'opportunities' -ControlType 'Subgrid' `
    -Label 'Related Opportunities' -Parameters $parameters -ColSpan 2 -PassThru
```

Creates a subgrid control showing related opportunities with custom parameters.

### Example 6: Add a web resource control
```powershell
PS C:\> $webResourceParams = @{
    'src' = 'WebResources/custom_chart.html'
    'height' = '300'
    'scrolling' = 'no'
}
PS C:\> Set-DataverseFormControl -Connection $c -FormId $formId -SectionName 'Analytics' `
    -DataField 'webresource_chart' -ControlType 'WebResource' `
    -Label 'Sales Chart' -Parameters $webResourceParams -ColSpan 2 -PassThru
```

Adds a web resource control displaying a custom HTML chart.

### Example 7: Update existing control properties
```powershell
PS C:\> Set-DataverseFormControl -Connection $c -FormId $formId -SectionName 'ContactInfo' `
    -ControlId 'existing-control-id' -DataField 'telephone1' `
    -Label 'Primary Phone' -IsRequired -ColSpan 1 -Visible
```

Updates an existing control's label, requirement status, and layout.

### Example 8: Create control using raw XML for advanced configuration
```powershell
PS C:\> $controlXml = @"
<control id="custom_datetime" classid="{5B773807-9FB2-42DB-97C3-7A91EFF8ADFF}" 
         datafieldname="custom_datetime" disabled="false" visible="true">
  <labels>
    <label description="Custom Date/Time" languagecode="1033" />
  </labels>
  <parameters>
    <DateAndTimeStyle>DateAndTime</DateAndTimeStyle>
    <HideTimeForDate>false</HideTimeForDate>
  </parameters>
</control>
"@
PS C:\> Set-DataverseFormControl -Connection $c -FormId $formId -SectionName 'CustomSection' `
    -ControlXml $controlXml -PassThru
```

Creates a datetime control using raw XML for full control over configuration.

### Example 9: Bulk control creation with error handling
```powershell
PS C:\> $controls = @(
    @{ DataField = 'firstname'; Label = 'First Name'; ColSpan = 1 }
    @{ DataField = 'lastname'; Label = 'Last Name'; ColSpan = 1; IsRequired = $true }
    @{ DataField = 'emailaddress1'; Label = 'Email'; ColSpan = 2; ControlType = 'Standard' }
)

PS C:\> foreach ($ctrl in $controls) {
    try {
        $params = @{
            Connection = $c
            FormId = $formId
            SectionName = 'ContactDetails'
            DataField = $ctrl.DataField
            Label = $ctrl.Label
            ColSpan = $ctrl.ColSpan
            ShowLabel = $true
            Visible = $true
            PassThru = $true
        }
        if ($ctrl.IsRequired) { $params.IsRequired = $true }
        if ($ctrl.ControlType) { $params.ControlType = $ctrl.ControlType }
        
        $controlId = Set-DataverseFormControl @params
        Write-Host "Created control: $($ctrl.DataField) with ID: $controlId"
    }
    catch {
        Write-Warning "Failed to create control $($ctrl.DataField): $($_.Exception.Message)"
    }
}
```

Creates multiple controls with error handling and conditional parameters.

## PARAMETERS

### -ColSpan
Number of columns the control spans in the section layout. Typical values are 1-4 depending on section column configuration.

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
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g.
http://server.com/MyOrg/).
If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -ControlId
ID of the control to update. When provided, the cmdlet updates an existing control instead of creating a new one.

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

### -ControlType
Control type for specialized behaviors. Valid values: Standard, Lookup, OptionSet, DateTime, Boolean, Subgrid, WebResource, QuickForm, Spacer, IFrame, Timer, KBSearch, Notes.

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: Standard, Lookup, OptionSet, DateTime, Boolean, Subgrid, WebResource, QuickForm, Spacer, IFrame, Timer, KBSearch, Notes

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ControlXml
Raw XML for the control element. Use this for advanced control configurations that require specific XML structure.

```yaml
Type: String
Parameter Sets: RawXml
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DataField
Data field name (attribute logical name) for the control. This binds the control to a specific entity attribute.

```yaml
Type: String
Parameter Sets: Update, Create
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Disabled
Whether the control is disabled (read-only) for users. Disabled controls display data but cannot be edited.

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
ID of the form containing the section where the control will be created or updated.

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
Zero-based index position to insert the control within the section. If not specified, the control is added at the end.

```yaml
Type: Int32
Parameter Sets: Create, RawXml
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InsertAfter
ID or data field name of the control after which to insert the new control. Cannot be used with Index or InsertBefore.

```yaml
Type: String
Parameter Sets: Create, RawXml
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InsertBefore
ID or data field name of the control before which to insert the new control. Cannot be used with Index or InsertAfter.

```yaml
Type: String
Parameter Sets: Create, RawXml
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IsRequired
Whether the control is required (must have a value before saving). This affects field validation and visual indicators.

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

### -Label
Label text for the control that will be displayed to users. Supports localization through LanguageCode parameter.

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
Language code for the label (default: 1033 for English US). Use this for localized forms.

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

### -Parameters
Custom parameters as a hashtable for special controls (Subgrid, WebResource, QuickForm, etc.). Parameters vary by control type.

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Return the control ID after creation or update. Useful for chaining operations or tracking created controls.

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
Publish the form after modifying the control. This makes changes visible to users immediately.

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

### -RowSpan
Number of rows the control spans in the section layout. Useful for controls that need more vertical space.

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

### -Rows
Number of rows for multiline text controls. Controls the height of text areas and memo fields.

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

### -SectionName
Name of the section containing the control. The section must exist in the form.

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

### -ShowLabel
Whether to show the control label. Default behavior varies by control type and form configuration.

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
Whether the control is visible to users. Hidden controls are not displayed but retain their data.

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
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

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
Controls how progress information is displayed during cmdlet execution.

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

### System.Guid
You can pipe form IDs to this cmdlet.

## OUTPUTS

### System.String
When -PassThru is specified, returns the control ID as a string.

## NOTES

**Control Types and Usage:**

| ControlType | Purpose | Common Parameters |
|-------------|---------|-------------------|
| **Standard** | Basic input fields | Rows (for multiline) |
| **Lookup** | Entity references | None (uses default lookup behavior) |
| **OptionSet** | Choice/Picklist fields | None (inherits from attribute) |
| **DateTime** | Date/time fields | DateAndTimeStyle, HideTimeForDate |
| **Boolean** | Yes/No fields | None (checkbox behavior) |
| **Subgrid** | Related records | targetEntityType, viewId, RelationshipName |
| **WebResource** | Custom HTML/images | src, height, width, scrolling |
| **QuickForm** | Embedded forms | EntityName, FormId |
| **Spacer** | Layout spacing | Height |
| **IFrame** | External content | src, height, width, scrolling |
| **Timer** | Date/time tracking | None |
| **KBSearch** | Knowledge base | None |
| **Notes** | Attachments | None |

**Parameter Sets:**
- **Create**: For creating new controls (no ControlId required)
- **Update**: For updating existing controls (ControlId required)
- **RawXml**: For advanced scenarios with custom XML

**Positioning Logic:**
- **Index**: Absolute position (0-based) within the section
- **InsertBefore**: Relative position before another control
- **InsertAfter**: Relative position after another control
- If none specified: Appends to the end of the section

**Layout Guidelines:**
- **ColSpan 1**: Single column width (typical for short fields)
- **ColSpan 2**: Double width (good for longer text, emails)
- **ColSpan 3-4**: Wide controls (descriptions, subgrids)
- **RowSpan**: Rarely used, mainly for custom controls

**Common Parameters by Control Type:**

**Subgrid Parameters:**
```powershell
@{
    'targetEntityType' = 'opportunity'
    'viewId' = 'view-guid'
    'RelationshipName' = 'contact_customer_opportunity'
    'EnableQuickFind' = 'true'
    'EnableViewPicker' = 'true'
    'RecordsPerPage' = '5'
}
```

**WebResource Parameters:**
```powershell
@{
    'src' = 'WebResources/custom_control.html'
    'height' = '300'
    'width' = '100%'
    'scrolling' = 'no'
    'border' = '0'
}
```

**DateTime Parameters:**
```powershell
@{
    'DateAndTimeStyle' = 'DateAndTime'  # or 'DateOnly'
    'HideTimeForDate' = 'false'
}
```

**Best Practices:**
- Use meaningful control IDs for easier maintenance
- Test control layouts on different screen sizes
- Consider user workflow when positioning controls
- Use appropriate ColSpan values for content
- Validate required field combinations
- Test with different user roles and permissions

## RELATED LINKS

[Get-DataverseFormControl](Get-DataverseFormControl.md)

[Remove-DataverseFormControl](Remove-DataverseFormControl.md)

[Get-DataverseFormSection](Get-DataverseFormSection.md)

[Set-DataverseFormSection](Set-DataverseFormSection.md)

[Get-DataverseForm](Get-DataverseForm.md)

[Set-DataverseForm](Set-DataverseForm.md)
