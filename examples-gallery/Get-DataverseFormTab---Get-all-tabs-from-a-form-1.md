---
title: "Get-DataverseFormTab - Get all tabs from a form"
tags: ['Metadata']
source: "Get-DataverseFormTab.md"
---
Retrieves all tabs from the contact Information form.

```powershell
$form = Get-DataverseForm -Entity 'contact' -Name 'Information'
Get-DataverseFormTab -FormId $form.Id

```

