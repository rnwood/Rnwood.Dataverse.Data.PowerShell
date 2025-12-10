---
title: "Remove-DataverseAttributeMetadata - Cleanup unused test fields"
tags: ['Metadata']
source: "Remove-DataverseAttributeMetadata.md"
---
Attempts to delete multiple test fields, handling cases where they may not exist.

```powershell
$testFields = @("new_test1", "new_test2", "new_debug1", "new_temp")
foreach ($field in $testFields) {
    Write-Host "Checking $field..."
    try {
        Remove-DataverseAttributeMetadata -EntityName account -AttributeName $field `
           -Force -ErrorAction Stop
        Write-Host "  Deleted $field" -ForegroundColor Green
    } catch {
        Write-Host "  Could not delete $field (may not exist or has dependencies)" `
           -ForegroundColor Yellow
    }
}

```
