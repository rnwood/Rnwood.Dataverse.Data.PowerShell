---
title: "Set-DataverseFormSection - Create section with cell formatting"
tags: ['Metadata']
source: "Set-DataverseFormSection.md"
---
Creates a section with centered labels positioned above controls.

```powershell
Set-DataverseFormSection -FormId $formId -TabName 'Advanced' `
   -Name 'FormattedSection' -Label 'Formatted Information' `
   -CellLabelAlignment Center -CellLabelPosition Top -Columns 2 -PassThru

```

