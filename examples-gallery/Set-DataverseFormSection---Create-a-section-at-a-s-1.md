---
title: "Set-DataverseFormSection - Create a section at a specific position"
tags: ['Metadata']
source: "Set-DataverseFormSection.md"
---
Creates a new section positioned after the 'ContactSection' with 2-column layout.

```powershell
Set-DataverseFormSection -FormId $formId -TabName 'Details' `
   -Name 'AddressSection' -Label 'Address Information' `
   -InsertAfter 'ContactSection' -Columns 2 -PassThru

```

