---
title: "Set-DataverseFormSection - Create hidden section for conditional display"
tags: ['Metadata']
source: "Set-DataverseFormSection.md"
---
Creates a hidden section that can be shown conditionally via business rules or JavaScript.

```powershell
Set-DataverseFormSection -FormId $formId -TabName 'Advanced' `
   -Name 'ConditionalSection' -Label 'Advanced Settings' `
   -Hidden -Columns 1 -ShowLabel:$false -PassThru

```

