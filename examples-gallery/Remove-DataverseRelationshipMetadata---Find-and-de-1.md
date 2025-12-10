---
title: "Remove-DataverseRelationshipMetadata - Find and delete relationships for an entity"
tags: ['Metadata']
source: "Remove-DataverseRelationshipMetadata.md"
---
Finds and deletes all test relationships (starting with "new_test") for an entity.

```powershell
$relationships = Get-DataverseRelationshipMetadata -EntityName new_customentity

$relationships | Where-Object { $_.SchemaName -like "new_test*" } | ForEach-Object {
    Write-Host "Deleting relationship: $($_.SchemaName)"
    Remove-DataverseRelationshipMetadata -SchemaName $_.SchemaName -Force
}

```
