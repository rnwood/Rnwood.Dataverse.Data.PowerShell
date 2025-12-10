---
title: "Set-DataverseFormSection - Create a section in specific column"
tags: ['Metadata']
source: "Set-DataverseFormSection.md"
---
Creates a section in the second column (index 1) of the tab with custom label width.

```powershell
Set-DataverseFormSection -FormId $formId -TabName 'General' `
   -Name 'RightPanelSection' -Label 'Additional Details' `
   -ColumnIndex 1 -Columns 1 -LabelWidth 120 -PassThru

```

