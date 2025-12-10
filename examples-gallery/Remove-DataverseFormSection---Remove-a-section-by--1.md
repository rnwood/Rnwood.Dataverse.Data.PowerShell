---
title: "Remove-DataverseFormSection - Remove a section by ID with immediate publishing"
tags: ['Metadata']
source: "Remove-DataverseFormSection.md"
---
Removes the section with the specified ID from the Details tab and publishes the form to make changes visible immediately.

```powershell
Remove-DataverseFormSection -FormId $formId -TabName 'Details' -SectionId 'section-guid-12345' -Publish

```

