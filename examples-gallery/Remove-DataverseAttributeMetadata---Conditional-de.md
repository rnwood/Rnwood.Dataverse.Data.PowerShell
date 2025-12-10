---
title: "Remove-DataverseAttributeMetadata - Conditional deletion based on data"
tags: ['Metadata']
source: "Remove-DataverseAttributeMetadata.md"
---
Checks if an attribute contains data before deleting it.

```powershell
$records = Get-DataverseRecord -TableName account -Columns new_customfield
$hasData = $records | Where-Object { $null -ne $_.new_customfield }

if (-not $hasData) {
    Write-Host "Field is empty in all records, safe to delete"
    Remove-DataverseAttributeMetadata -EntityName account -AttributeName new_customfield -Force
} else {
    Write-Warning "Field contains data in $($hasData.Count) records. Review before deletion."
}

```
