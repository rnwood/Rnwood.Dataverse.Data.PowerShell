---
title: "Remove-DataverseRelationshipMetadata - Conditional deletion based on usage"
tags: ['Metadata']
source: "Remove-DataverseRelationshipMetadata.md"
---
Checks if a ManyToMany relationship has any associations before deleting.

```powershell
$rel = Get-DataverseRelationshipMetadata -SchemaName new_project_contact

if ($rel.RelationshipType -eq "ManyToMany") {
    # Check if there are any associations
    $query = "SELECT COUNT(*) as Count FROM $($rel.IntersectEntityName)"
    $count = (Invoke-DataverseSql -Sql $query).Count
    
    if ($count -eq 0) {
        Write-Host "No associations found, safe to delete"
        Remove-DataverseRelationshipMetadata -SchemaName new_project_contact -Force
    } else {
        Write-Warning "Relationship has $count associations. Review before deletion."
    }
}

```
