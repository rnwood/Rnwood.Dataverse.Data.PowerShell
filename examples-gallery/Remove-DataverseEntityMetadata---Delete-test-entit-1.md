---
title: "Remove-DataverseEntityMetadata - Delete test entities matching a pattern"
tags: ['Metadata']
source: "Remove-DataverseEntityMetadata.md"
---
Finds and deletes all entities whose names start with "new_test".

```powershell
Get-DataverseEntityMetadata onn | 
    Where-Object { $_.LogicalName -like "new_test*" } | 
    ForEach-Object { 
        Write-Host "Deleting $($_.LogicalName)..."
        Remove-DataverseEntityMetadata -EntityName $_.LogicalName -Force 
    }

```

