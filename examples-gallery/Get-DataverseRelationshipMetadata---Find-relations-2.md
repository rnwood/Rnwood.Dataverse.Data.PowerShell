---
title: "Get-DataverseRelationshipMetadata - Find relationships by entity pattern"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Finds all relationships related to project entities.

```powershell
$projectRelationships = Get-DataverseRelationshipMetadata | 
    Where-Object { $_.SchemaName -like "*project*" }

$projectRelationships | Select-Object SchemaName, RelationshipType, ReferencedEntity, ReferencingEntity

# SchemaName                    RelationshipType ReferencedEntity ReferencingEntity
# ----------                   ---------------- ----------------- ------------------
# new_project_contact           OneToMany        new_project      contact
# new_project_task              OneToMany        new_project      new_task
# new_project_resource          ManyToMany       new_project      new_resource
# new_project_milestone         OneToMany        new_project      new_milestone

```
