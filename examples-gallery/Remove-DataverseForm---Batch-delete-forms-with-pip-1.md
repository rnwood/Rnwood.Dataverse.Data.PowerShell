---
title: "Remove-DataverseForm - Batch delete forms with pipeline"
tags: ['Metadata']
source: "Remove-DataverseForm.md"
---
Finds and deletes all contact forms whose names start with 'Test'.

```powershell
Get-DataverseForm -Entity 'contact' | 
    Where-Object { $_.Name -like 'Test*' } | 
    Remove-DataverseForm -IfExists -Confirm:$false

```

