---
title: "Set-DataverseFormControl - Update existing control properties"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Updates an existing control's label, requirement status, and layout.

```powershell
Set-DataverseFormControl -FormId $formId -SectionName 'ContactInfo' `
   -ControlId 'existing-control-id' -DataField 'telephone1' `
   -Labels @{1033 = 'Primary Phone'} -IsRequired -ColSpan 1 -Visible

```

