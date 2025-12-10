---
title: "Set-DataverseFormControl - Create multiple header controls"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Creates multiple controls in the form header with specific positioning.

```powershell
# Create email control in header
Set-DataverseFormControl -FormId $formId `
   -TabName '[Header]' -DataField 'emailaddress1' `
   -ControlType 'Email' -Labels @{1033 = 'Email'} -ColSpan 1 -Disabled

# Create owner control in header
Set-DataverseFormControl -FormId $formId `
   -TabName '[Header]' -DataField 'ownerid' `
   -ControlType 'Lookup' -Labels @{1033 = 'Owner'} -ColSpan 1 -Disabled

# Get all header controls
Get-DataverseFormControl -FormId $formId -TabName '[Header]'

```

