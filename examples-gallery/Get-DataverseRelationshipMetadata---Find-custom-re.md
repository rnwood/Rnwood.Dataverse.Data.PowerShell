---
title: "Get-DataverseRelationshipMetadata - Find custom relationships"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Finds all custom relationships in the organization.

```powershell
$customRelationships = Get-DataverseRelationshipMetadata | 
    Where-Object { $_.IsCustomRelationship -eq $true }

$customRelationships | Select-Object SchemaName, RelationshipType, ReferencedEntity, ReferencingEntity

# SchemaName                    RelationshipType ReferencedEntity ReferencingEntity
# ----------                   ---------------- ----------------- ------------------
# new_project_contact           OneToMany        new_project      contact
# new_project_task              OneToMany        new_project      new_task
# new_project_resource          ManyToMany       new_project      new_resource

```
