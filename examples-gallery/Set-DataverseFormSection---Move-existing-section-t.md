---
title: "Set-DataverseFormSection - Move existing section to different position"
tags: ['Metadata']
source: "Set-DataverseFormSection.md"
---
Moves an existing section to the first position (index 0) within its column.

```powershell
$section = Get-DataverseFormSection -FormId $formId -TabName 'General' -SectionName 'Summary'
Set-DataverseFormSection -FormId $formId -TabName 'General' `
   -SectionId $section.Id -Name 'Summary' -Index 0

```

