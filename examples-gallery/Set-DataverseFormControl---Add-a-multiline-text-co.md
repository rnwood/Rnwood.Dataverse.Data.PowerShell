---
title: "Set-DataverseFormControl - Add a multiline text control"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Creates a multiline text area with 4 rows spanning 2 columns.

```powershell
Set-DataverseFormControl -FormId $formId -SectionName 'Details' `
   -DataField 'description' -ControlType 'Standard' `
   -Labels @{1033 = 'Description'} -Rows 4 -ColSpan 2 -PassThru

```

