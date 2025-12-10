---
title: "Remove-DataverseFormTab - Remove a tab by name"
tags: ['Metadata']
source: "Remove-DataverseFormTab.md"
---
Removes the tab named 'CustomTab' from the contact Information form.

```powershell
$form = Get-DataverseForm -Entity 'contact' -Name 'Information'
Remove-DataverseFormTab -FormId $form.Id -TabName 'CustomTab'

```

