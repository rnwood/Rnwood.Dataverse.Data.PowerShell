---
title: "Get-DataverseRelationshipMetadata - Get a specific relationship by name"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Retrieves metadata for a specific relationship by schema name.

```powershell
$rel = Get-DataverseRelationshipMetadata -RelationshipName account_primary_contact
[PSCustomObject]@{
    SchemaName = $rel.SchemaName
    RelationshipType = $rel.RelationshipType
    ReferencedEntity = $rel.ReferencedEntity
    ReferencingEntity = $rel.ReferencingEntity
    CascadeDelete = $rel.CascadeDelete.Value
    IsCustomRelationship = $rel.IsCustomRelationship
}

# SchemaName            : account_primary_contact
# RelationshipType      : OneToMany
# ReferencedEntity      : account
# ReferencingEntity     : contact
# CascadeDelete         : RemoveLink
# IsCustomRelationship  : False

```
