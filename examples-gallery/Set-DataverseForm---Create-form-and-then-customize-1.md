---
title: "Set-DataverseForm - Create form and then customize with specialized cmdlets"
tags: ['Metadata']
source: "Set-DataverseForm.md"
---
Demonstrates creating a form and then customizing it with specialized form cmdlets.

```powershell
# Create basic form
$formId = Set-DataverseForm -Entity 'contact' -Name 'My Custom Form' -FormType 'Main' -PassThru

# Add a tab
$tabId = Set-DataverseFormTab -FormId $formId -Name 'CustomTab' -Label 'Custom Information' -PassThru

# Add a section to the tab
$sectionId = Set-DataverseFormSection -FormId $formId -TabName 'CustomTab' -Name 'CustomSection' -Label 'Additional Details' -PassThru

# Add controls to the section
Set-DataverseFormControl -FormId $formId -TabName 'CustomTab' -SectionName 'CustomSection' -DataField 'description' -Label 'Notes'

# Publish the form
Set-DataverseForm -Id $formId -Publish

```

