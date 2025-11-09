---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseFormControl

## SYNOPSIS
Retrieves control information from a Dataverse form.

## SYNTAX

```
Get-DataverseFormControl -FormId <Guid> [-TabName <String>] [-SectionName <String>] [-ControlId <String>] 
[-DataField <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseFormControl cmdlet retrieves control information from a Dataverse form. It parses the FormXML to extract control details including ID, data field, class ID, labels, events, parameters, and visibility settings. You can retrieve all controls from a form or filter by tab, section, control ID, or data field name.

## EXAMPLES

### Example 1: Get all controls from a form
```powershell
PS C:\> $form = Get-DataverseForm -Connection $c -Entity 'contact' -Name 'Information'
PS C:\> Get-DataverseFormControl -Connection $c -FormId $form.FormId
```

Retrieves all controls from all tabs and sections in the contact Information form.

### Example 2: Get controls from a specific section
```powershell
PS C:\> $formId = '12345678-1234-1234-1234-123456789012'
PS C:\> Get-DataverseFormControl -Connection $c -FormId $formId -SectionName 'ContactDetails'
```

Retrieves all controls from the 'ContactDetails' section.

### Example 3: Get a specific control by ID
```powershell
PS C:\> Get-DataverseFormControl -Connection $c -FormId $formId -ControlId 'firstname_ctrl'
```

Retrieves the control with ID 'firstname_ctrl'.

### Example 4: Get controls by data field
```powershell
PS C:\> Get-DataverseFormControl -Connection $c -FormId $formId -DataField 'emailaddress1'
```

Retrieves all controls bound to the 'emailaddress1' field.

### Example 5: Get controls from specific tab and section
```powershell
PS C:\> Get-DataverseFormControl -Connection $c -FormId $formId -TabName 'Details' -SectionName 'ContactInfo'
```

Retrieves controls from the 'ContactInfo' section within the 'Details' tab.

### Example 6: Explore control properties
```powershell
PS C:\> $control = Get-DataverseFormControl -Connection $c -FormId $formId -DataField 'firstname'
PS C:\> $control.Id              # Control ID
PS C:\> $control.DataField       # Bound data field name
PS C:\> $control.ClassId         # Control class identifier
PS C:\> $control.Labels          # Localized labels
PS C:\> $control.Events          # Event handlers
PS C:\> $control.Parameters      # Custom parameters
```

Explores the structure and properties of a specific control.

### Example 7: Find controls with specific properties
```powershell
PS C:\> $controls = Get-DataverseFormControl -Connection $c -FormId $formId
PS C:\> $controls | Where-Object { $_.Hidden -eq $true }                    # Hidden controls
PS C:\> $controls | Where-Object { $_.IsRequired -eq $true }               # Required controls
PS C:\> $controls | Where-Object { $_.Disabled -eq $true }                 # Disabled controls
PS C:\> $controls | Where-Object { $_.ShowLabel -eq $false }               # Controls without labels
```

Filters controls based on specific properties.

### Example 8: Get controls and their locations
```powershell
PS C:\> Get-DataverseFormControl -Connection $c -FormId $formId | 
    Select-Object TabName, SectionName, Id, DataField | 
    Sort-Object TabName, SectionName, Id
```

Gets controls with their location information and sorts by hierarchy.

## PARAMETERS

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
ID of a specific control to retrieve. If not specified, all controls are returned.

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

### -DataField
Data field name to filter controls. Returns all controls bound to the specified field.

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

### -FormId
ID of the form to retrieve controls from.

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

### -SectionName
Name of the section containing the controls. If not specified, controls from all sections are returned.

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

### -TabName
Name of the tab containing the sections. If not specified, controls from all tabs are returned.

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

### System.Management.Automation.PSObject
Returns PSObjects with the following properties:
- **Id**: Control ID (string)
- **DataField**: Name of the bound data field/attribute (string)
- **ClassId**: Control class identifier (string)
- **FormId**: ID of the containing form (Guid)
- **TabName**: Name of the containing tab (string)
- **SectionName**: Name of the containing section (string)
- **Disabled**: Whether the control is disabled (boolean)
- **Hidden**: Whether the control is hidden (boolean, opposite of Visible)
- **ShowLabel**: Whether to show the control label (boolean)
- **IsRequired**: Whether the control is marked as required (boolean)
- **Labels**: Array of localized labels with Description and LanguageCode
- **Events**: Event handlers configured for the control (PSObject)
- **Parameters**: Custom parameters for the control (PSObject)

## NOTES

**Form Structure Hierarchy:**
```
Form
??? Header
??? Tabs
?   ??? Tab 1
?   ?   ??? Columns
?   ?   ?   ??? Column 1
?   ?   ?   ?   ??? Section 1
?   ?   ?   ?   ?   ??? Row 1
?   ?   ?   ?   ?   ?   ??? Cell 1 ? Control 1
?   ?   ?   ?   ?   ?   ??? Cell 2 ? Control 2
?   ?   ?   ?   ?   ??? Row 2
?   ?   ?   ?   ??? Section 2
?   ?   ?   ??? Column 2
?   ?   ??? Controls (within sections)
?   ??? Tab 2
??? Footer/Navigation
```

**Control Types:**
Controls are identified by their ClassId attribute which determines behavior:
- Standard text/numeric input controls
- Lookup controls for entity references
- Option set controls for choice fields
- Date/time pickers
- Boolean (checkbox) controls
- Subgrids for related records
- Web resources for custom content
- Quick forms for embedded forms
- Spacer controls for layout
- Timer controls for time tracking

**Control Properties:**
- **DataField**: The logical name of the entity attribute the control is bound to
- **ClassId**: GUID that identifies the control type and behavior
- **Disabled**: Whether the control accepts input
- **Hidden**: Whether the control is visible on the form
- **ShowLabel**: Whether to display the field label
- **IsRequired**: Whether the control enforces required field validation

**Common Use Cases:**
- Form control auditing and analysis
- Control-level customization validation
- Automated form configuration documentation
- Cross-environment form comparison
- Finding controls by data field across forms

**Performance Notes:**
- Retrieves unpublished form version first, falls back to published
- Parses FormXML efficiently with minimal performance impact
- Use filter parameters for better performance on forms with many controls

## RELATED LINKS

[Get-DataverseForm](Get-DataverseForm.md)

[Get-DataverseFormTab](Get-DataverseFormTab.md)

[Get-DataverseFormSection](Get-DataverseFormSection.md)

[Set-DataverseFormControl](Set-DataverseFormControl.md)

[Remove-DataverseFormControl](Remove-DataverseFormControl.md)
