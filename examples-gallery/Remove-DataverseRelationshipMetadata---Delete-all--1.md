---
title: "Remove-DataverseRelationshipMetadata - Delete all custom relationships for cleanup"
tags: ['Metadata']
source: "Remove-DataverseRelationshipMetadata.md"
---
Finds and attempts to delete all custom relationships, handling errors for dependencies.

```powershell
Get-DataverseRelationshipMetadata | 
    Where-Object { $_.IsCustomRelationship -and $_.SchemaName -like "new_*" } |
    ForEach-Object {
        Write-Host "Deleting $($_.SchemaName) ($($_.RelationshipType))..."
        try {
            Remove-DataverseRelationshipMetadata -SchemaName $_.SchemaName `
               -Force -ErrorAction Stop
            Write-Host "  Deleted successfully" -ForegroundColor Green
        } catch {
            Write-Warning "  Could not delete: $($_.Exception.Message)"
        }
    }

```
