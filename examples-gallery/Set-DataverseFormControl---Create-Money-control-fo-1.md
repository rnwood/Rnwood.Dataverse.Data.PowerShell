---
title: "Set-DataverseFormControl - Create Money control for currency field"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Creates a currency control with proper formatting.

```powershell
Set-DataverseFormControl -FormId $formId `
   -TabName 'Financial' -SectionName 'Pricing' `
   -DataField 'revenue' -ControlType 'Money' `
   -Labels @{1033 = 'Annual Revenue'} -ColSpan 1 -PassThru

```

