---
title: "Get-DataverseFormSection - Get all sections from a form"
tags: ['Metadata']
source: "Get-DataverseFormSection.md"
---
Retrieves all sections from all tabs in the contact Information form.

```powershell
$form = Get-DataverseForm -Entity 'contact' -Name 'Information'
Get-DataverseFormSection -FormId $form.FormId

```

