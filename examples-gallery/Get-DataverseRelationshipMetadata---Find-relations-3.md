---
title: "Get-DataverseRelationshipMetadata - Find relationships with specific cascade behavior"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Finds relationships that prevent deletion of parent records when child records exist.

```powershell
$restrictDelete = Get-DataverseRelationshipMetadata -RelationshipType OneToMany | 
    Where-Object { $_.CascadeDelete.Value -eq "Restrict" }

$restrictDelete | Select-Object SchemaName, ReferencedEntity, ReferencingEntity

# SchemaName                    ReferencedEntity ReferencingEntity
# ----------                   ----------------- ------------------
# account_account_master_account account          account
# contact_contact_master_contact contact          contact

```
