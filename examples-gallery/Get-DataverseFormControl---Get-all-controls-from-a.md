---
title: "Get-DataverseFormControl - Get all controls from a form"
tags: ['Metadata']
source: "Get-DataverseFormControl.md"
---
Retrieves all controls from all tabs, sections, and the header in the contact Information form.

```powershell
$form = Get-DataverseForm -Entity 'contact' -Name 'Information'
Get-DataverseFormControl -FormId $form.FormId

```

