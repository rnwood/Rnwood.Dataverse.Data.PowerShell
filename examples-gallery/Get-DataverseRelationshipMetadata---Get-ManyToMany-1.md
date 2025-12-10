---
title: "Get-DataverseRelationshipMetadata - Get ManyToMany relationships for an entity"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Retrieves ManyToMany relationships for the account entity.

```powershell
$manyToMany = Get-DataverseRelationshipMetadata -EntityName account -RelationshipType ManyToMany
$manyToMany | Select-Object SchemaName, ReferencedEntity, ReferencingEntity, IntersectEntityName

# SchemaName                    ReferencedEntity ReferencingEntity IntersectEntityName
# ----------                   ----------------- ----------------- -------------------
# accountleads_association      account          lead             accountleads
# accountopportunities_association account        opportunity      accountopportunities
# accountcompetitors_association account          competitor       accountcompetitors

```
