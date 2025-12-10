---
title: "Remove-DataverseForm - Safe deletion with IfExists"
tags: ['Metadata']
source: "Remove-DataverseForm.md"
---
Attempts to delete a form but doesn't raise an error if the form doesn't exist.

```powershell
Remove-DataverseForm -Entity 'account' -Name 'Test Form' -IfExists -Confirm:$false

```

