
# Form Management

This guide covers managing Dataverse forms and form components using PowerShell cmdlets.

## Overview

Forms are the primary user interface for working with data in Dataverse. This module provides comprehensive cmdlets for:

- **Form CRUD operations** - Create, retrieve, update, and delete forms
- **Form component management** - Manage tabs, sections, and controls within forms
- **FormXml manipulation** - Work with form definitions programmatically
- **Positioning control** - Insert components at specific positions
- **Raw XML support** - Full control over FormXml structure

## Form Types

Dataverse supports several form types:

- **Main** - Standard entity forms
- **QuickCreate** - Quick create forms
- **QuickView** - Quick view forms (read-only)
- **Card** - Card forms for mobile
- **Dashboard** - Dashboard forms
- **Other specialized types** - Dialog, Task Flow Card, Main Interactive Experience, etc.

## Basic Form Operations

### Retrieving Forms

Get forms by ID, entity name, or entity+name:

```powershell
# Get all forms for an entity
Get-DataverseForm -Connection $conn -EntityName "contact"

# Get a specific form by ID
Get-DataverseForm -Connection $conn -FormId "5c1e7e7d-7a3e-4b0f-8f9a-1a2b3c4d5e6f"

# Get a specific form by entity and name
Get-DataverseForm -Connection $conn -EntityName "contact" -FormName "Information"

# Filter by form type
Get-DataverseForm -Connection $conn -EntityName "contact" | Where-Object { $_.Type -eq "Main" }

# Include FormXml for manipulation
$form = Get-DataverseForm -Connection $conn -FormId $formId -IncludeFormXml
```

### Creating and Updating Forms

Create new forms or update existing ones:

```powershell
# Create a new form with FormXml
$formXml = @'
<form>
  <tabs>
    <tab name="general" id="general_tab">
      <labels>
        <label description="General" languagecode="1033" />
      </labels>
      <columns>
        <column width="100%">
          <sections>
            <section name="general_section" showlabel="false" showbar="false">
              <labels>
                <label description="General Information" languagecode="1033" />
              </labels>
            </section>
          </sections>
        </column>
      </columns>
    </tab>
  </tabs>
</form>
'@

Set-DataverseForm -Connection $conn `
    -EntityName "contact" `
    -FormName "Custom Contact Form" `
    -FormXml $formXml `
    -Type "Main" `
    -Publish

# Update existing form
Set-DataverseForm -Connection $conn `
    -FormId $formId `
    -FormName "Updated Name" `
    -Description "Updated description" `
    -Publish
```

### Deleting Forms

Remove forms with optional publishing:

```powershell
# Delete a form by ID
Remove-DataverseForm -Connection $conn -FormId $formId -Publish

# Delete with confirmation bypass
Remove-DataverseForm -Connection $conn -FormId $formId -Publish -Confirm:$false

# Delete with WhatIf to preview
Remove-DataverseForm -Connection $conn -FormId $formId -WhatIf

# Delete with IfExists flag (no error if not found)
Remove-DataverseForm -Connection $conn -FormId $formId -IfExists
```

## Working with Form Components

### Tabs

Tabs organize form content into logical groups.

```powershell
# Get all tabs from a form
$tabs = Get-DataverseFormTab -Connection $conn -FormId $formId

# Create or update a tab
Set-DataverseFormTab -Connection $conn `
    -FormId $formId `
    -Name "custom_tab" `
    -Label "Custom Information" `
    -Expanded $true `
    -Publish

# Insert tab at specific position
Set-DataverseFormTab -Connection $conn `
    -FormId $formId `
    -Name "first_tab" `
    -Label "First Tab" `
    -Index 0 `
    -Publish

# Insert before another tab
Set-DataverseFormTab -Connection $conn `
    -FormId $formId `
    -Name "middle_tab" `
    -Label "Middle Tab" `
    -InsertBefore "general_tab" `
    -Publish

# Remove a tab
Remove-DataverseFormTab -Connection $conn `
    -FormId $formId `
    -Name "custom_tab" `
    -Publish
```

### Sections

Sections group controls within a tab.

```powershell
# Get all sections from a form
$sections = Get-DataverseFormSection -Connection $conn -FormId $formId

# Get sections from a specific tab
$sections = Get-DataverseFormSection -Connection $conn -FormId $formId -TabName "general_tab"

# Create or update a section
Set-DataverseFormSection -Connection $conn `
    -FormId $formId `
    -TabName "general_tab" `
    -Name "contact_details" `
    -Label "Contact Details" `
    -ShowLabel $true `
    -ShowBar $true `
    -Publish

# Insert section at specific position
Set-DataverseFormSection -Connection $conn `
    -FormId $formId `
    -TabName "general_tab" `
    -Name "top_section" `
    -Label "Top Section" `
    -Index 0 `
    -Publish

# Remove a section (TabName required for disambiguation)
Remove-DataverseFormSection -Connection $conn `
    -FormId $formId `
    -TabName "general_tab" `
    -Name "contact_details" `
    -Publish
```

### Controls

Controls are the individual fields and components on a form.

```powershell
# Get all controls from a form
$controls = Get-DataverseFormControl -Connection $conn -FormId $formId

# Get controls from a specific section (requires TabName)
$controls = Get-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -TabName "general_tab" `
    -SectionName "contact_details"

# Add a standard text field
Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -TabName "general_tab" `
    -SectionName "contact_details" `
    -DataField "firstname" `
    -Label "First Name" `
    -Publish

# Add a lookup control
Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -TabName "general_tab" `
    -SectionName "contact_details" `
    -DataField "parentcustomerid" `
    -Label "Company Name" `
    -ControlType "Lookup" `
    -Publish

# Add control at specific position
Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -TabName "general_tab" `
    -SectionName "contact_details" `
    -DataField "lastname" `
    -InsertAfter "firstname" `
    -Publish

# Remove a control by DataField
Remove-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -TabName "general_tab" `
    -SectionName "contact_details" `
    -DataField "emailaddress1" `
    -Publish

# Remove a control by ID
Remove-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -ControlId "email_ctrl" `
    -Publish
```

## Advanced Control Types

### Option Set (Dropdown)

```powershell
Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -TabName "general_tab" `
    -SectionName "details" `
    -DataField "preferredcontactmethodcode" `
    -Label "Preferred Method" `
    -ControlType "OptionSet" `
    -Publish
```

### Boolean (Two Options)

```powershell
Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -TabName "general_tab" `
    -SectionName "details" `
    -DataField "donotemail" `
    -Label "Do Not Email" `
    -ControlType "Boolean" `
    -Publish
```

### Subgrid

```powershell
Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -TabName "related_tab" `
    -SectionName "activities" `
    -DataField "activities" `
    -Label "Activities" `
    -ControlType "Subgrid" `
    -Publish
```

### Web Resource

```powershell
Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -TabName "custom_tab" `
    -SectionName "web_content" `
    -DataField "new_webresource" `
    -Label "Custom Content" `
    -ControlType "WebResource" `
    -Publish
```

### Quick View Form

```powershell
Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -TabName "related_tab" `
    -SectionName "account_info" `
    -DataField "parentcustomerid" `
    -Label "Account Quick View" `
    -ControlType "QuickForm" `
    -Publish
```

## Raw XML Control Manipulation

For advanced scenarios, you can provide complete control XML:

```powershell
$controlXml = @'
<control id="new_customfield" 
         datafieldname="new_customfield" 
         classid="{4273EDBD-AC1D-40d3-9FB2-095C621B552D}">
  <labels>
    <label description="Custom Field" languagecode="1033" />
  </labels>
  <parameters>
    <MaxLength>100</MaxLength>
    <Format>Text</Format>
    <IMEMode>Auto</IMEMode>
  </parameters>
</control>
'@

Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -TabName "general_tab" `
    -SectionName "custom_section" `
    -ControlXml $controlXml `
    -InsertAfter "firstname" `
    -Publish
```

## Positioning Strategies

All Set- cmdlets support flexible positioning:

### By Index

```powershell
# Insert at the beginning (index 0)
Set-DataverseFormTab -FormId $formId -Name "first_tab" -Index 0

# Insert at specific position
Set-DataverseFormSection -FormId $formId -TabName "general" -Name "section2" -Index 1
```

### Relative Positioning

```powershell
# Insert before an element
Set-DataverseFormTab -FormId $formId -Name "new_tab" -InsertBefore "existing_tab"

# Insert after an element
Set-DataverseFormControl -FormId $formId -DataField "lastname" -InsertAfter "firstname"
```

### Default Behavior

If no positioning parameter is specified, elements are added at the end.

## Important Considerations

### Section Name Scoping

**Section names are only unique within a tab.** When using `SectionName` to locate a section:

- `Get-DataverseFormControl` requires `TabName` when `SectionName` is specified
- `Set-DataverseFormControl` requires `TabName` with `SectionName`
- `Remove-DataverseFormControl` requires `TabName` when using `DataField` parameter

Example:

```powershell
# ✅ Correct - TabName provided with SectionName
Get-DataverseFormControl -FormId $formId -TabName "general_tab" -SectionName "contact_details"

# ❌ Error - SectionName without TabName
Get-DataverseFormControl -FormId $formId -SectionName "contact_details"
```

### Publishing

Form changes are not visible to users until published:

```powershell
# Publish immediately after changes
Set-DataverseFormTab -FormId $formId -Name "new_tab" -Publish

# Make multiple changes, then publish once
Set-DataverseFormTab -FormId $formId -Name "tab1" -Label "Tab 1"
Set-DataverseFormTab -FormId $formId -Name "tab2" -Label "Tab 2"
Set-DataverseForm -FormId $formId -Publish
```

### WhatIf and Confirm

Test changes safely before applying:

```powershell
# Preview what would be deleted
Remove-DataverseFormTab -FormId $formId -Name "old_tab" -WhatIf

# Bypass confirmation for automation
Remove-DataverseFormTab -FormId $formId -Name "old_tab" -Confirm:$false
```

## Best Practices

1. **Always specify TabName with SectionName** - Section names are not globally unique
2. **Use WhatIf for testing** - Preview destructive operations before applying
3. **Batch changes before publishing** - Minimize publishes for better performance
4. **Use meaningful names** - Use descriptive names and labels for maintainability
5. **Handle errors gracefully** - Use try/catch blocks and check for existing elements
6. **Test in development first** - Validate form changes in a development environment
7. **Document custom controls** - Keep notes on any custom control configurations

## Error Handling

```powershell
try {
    Set-DataverseFormControl -Connection $conn `
        -FormId $formId `
        -TabName "general_tab" `
        -SectionName "contact_details" `
        -DataField "firstname" `
        -Label "First Name" `
        -Publish
} catch {
    Write-Error "Failed to add control: $_"
}
```

## Complete Example

Here's a complete example creating a custom form with tabs, sections, and controls:

```powershell
$conn = Get-DataverseConnection -Interactive

# Create a new form
$formId = Set-DataverseForm -Connection $conn `
    -EntityName "contact" `
    -FormName "Custom Contact Form" `
    -Type "Main" `
    -Description "Custom form for contacts" `
    -PassThru

# Add a custom tab
Set-DataverseFormTab -Connection $conn `
    -FormId $formId `
    -Name "custom_tab" `
    -Label "Custom Information" `
    -Expanded $true

# Add a section to the tab
Set-DataverseFormSection -Connection $conn `
    -FormId $formId `
    -TabName "custom_tab" `
    -Name "custom_section" `
    -Label "Custom Details" `
    -ShowLabel $true

# Add controls to the section
Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -TabName "custom_tab" `
    -SectionName "custom_section" `
    -DataField "firstname" `
    -Label "First Name"

Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -TabName "custom_tab" `
    -SectionName "custom_section" `
    -DataField "lastname" `
    -Label "Last Name" `
    -InsertAfter "firstname"

Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -TabName "custom_tab" `
    -SectionName "custom_section" `
    -DataField "emailaddress1" `
    -Label "Email" `
    -InsertAfter "lastname"

# Publish all changes
Set-DataverseForm -Connection $conn -FormId $formId -Publish

Write-Host "Form created and published successfully!" -ForegroundColor Green
```

## See Also

- [Get-DataverseForm](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseForm.md)
- [Set-DataverseForm](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseForm.md)
- [Remove-DataverseForm](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseForm.md)
- [Get-DataverseFormTab](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseFormTab.md)
- [Set-DataverseFormTab](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseFormTab.md)
- [Remove-DataverseFormTab](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseFormTab.md)
- [Get-DataverseFormSection](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseFormSection.md)
- [Set-DataverseFormSection](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseFormSection.md)
- [Remove-DataverseFormSection](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseFormSection.md)
- [Get-DataverseFormControl](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseFormControl.md)
- [Set-DataverseFormControl](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseFormControl.md)
- [Remove-DataverseFormControl](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseFormControl.md)
- [Examples-Comparison.md](../../Examples-Comparison.md) - Side-by-side comparisons with Microsoft.Xrm.Data.PowerShell
