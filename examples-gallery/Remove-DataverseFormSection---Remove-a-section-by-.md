---
title: "Remove-DataverseFormSection - Remove a section by name"
tags: ['Metadata']
source: "Remove-DataverseFormSection.md"
---
Removes the section named 'CustomSection' from the General tab of the contact Information form.

```powershell
$form = Get-DataverseForm -Entity 'contact' -Name 'Information'
Remove-DataverseFormSection -FormId $form.Id -TabName 'General' -SectionName 'CustomSection'

```

