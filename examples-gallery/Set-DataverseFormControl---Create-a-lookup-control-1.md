---
title: "Set-DataverseFormControl - Create a lookup control with custom properties"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Creates a lookup control spanning 2 columns and marked as required.

```powershell
Set-DataverseFormControl -FormId $formId -SectionName 'ContactInfo' `
   -DataField 'parentcustomerid' -ControlType 'Lookup' `
   -Labels @{1033 = 'Parent Account'} -ColSpan 2 -IsRequired -PassThru

```

