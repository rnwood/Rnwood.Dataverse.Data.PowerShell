---
title: "Remove-DataverseRelationshipMetadata - Delete relationship with entity name for cache invalidation"
tags: ['Metadata']
source: "Remove-DataverseRelationshipMetadata.md"
---
Deletes a relationship and invalidates the metadata cache for the `new_project` entity.

```powershell
Remove-DataverseRelationshipMetadata -SchemaName new_project_task `
   -EntityName new_project -Force

```
