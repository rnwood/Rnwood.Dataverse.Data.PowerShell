---
title: "Set-DataverseFormControl - Update existing header control"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Updates an existing header control's label and cell span without recreating it.

```powershell
Set-DataverseFormControl -FormId $formId `
   -TabName '[Header]' -DataField 'emailaddress1' `
   -Labels @{1033 = 'Primary Email'} -ColSpan 2

```

