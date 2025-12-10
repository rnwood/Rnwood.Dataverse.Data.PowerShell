---
title: "Remove-DataverseEntityMetadata - Handle deletion errors"
tags: ['Metadata']
source: "Remove-DataverseEntityMetadata.md"
---
Handles potential errors during entity deletion with proper error handling.

```powershell
try {
    Remove-DataverseEntityMetadata -EntityName new_customentity -Force -ErrorAction Stop
    Write-Host "Entity deleted successfully"
} catch {
    Write-Error "Failed to delete entity: $($_.Exception.Message)"
}

```
