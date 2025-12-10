---
title: "Remove-DataverseEntityMetadata - Conditional deletion based on records"
tags: ['Metadata']
source: "Remove-DataverseEntityMetadata.md"
---
Checks if an entity is empty before deleting it.

```powershell
$recordCount = (Get-DataverseRecord -TableName new_customentity).Count
if ($recordCount -eq 0) {
    Write-Host "Entity is empty, safe to delete"
    Remove-DataverseEntityMetadata -EntityName new_customentity -Force
} else {
    Write-Warning "Entity contains $recordCount records. Deletion cancelled."
}

```
