---
title: "Set-DataverseFormControl - Create control with cell-level attributes"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Creates a memo control with cell-level attributes including auto-sizing and custom cell ID.

```powershell
Set-DataverseFormControl -FormId $formId `
   -TabName 'General' -SectionName 'Details' `
   -DataField 'description' -ControlType 'Memo' `
   -Labels @{1033 = 'Notes'} -ColSpan 2 -RowSpan 2 `
   -Auto -LockLevel 0 -CellId 'cell_description' -PassThru

```

