---
title: "Set-DataverseFormSection - Update an existing section's properties"
tags: ['Metadata']
source: "Set-DataverseFormSection.md"
---
Updates an existing section's label and layout properties.

```powershell
$section = Get-DataverseFormSection -FormId $formId -TabName 'General' -SectionName 'ContactDetails'
Set-DataverseFormSection -FormId $formId -TabName 'General' `
   -SectionId $section.Id -Name 'ContactDetails' `
   -Label 'Updated Contact Information' -ShowLabel -ShowBar -Columns 1

```

