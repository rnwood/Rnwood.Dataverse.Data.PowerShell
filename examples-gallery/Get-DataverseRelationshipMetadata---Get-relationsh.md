---
title: "Get-DataverseRelationshipMetadata - Get relationships with cascade delete"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Finds relationships that will delete related records when the parent is deleted.

```powershell
$cascadeDeleteRels = Get-DataverseRelationshipMetadata -RelationshipType OneToMany | 
    Where-Object { $_.CascadeDelete.Value -eq "Cascade" }

$cascadeDeleteRels | Select-Object SchemaName, ReferencedEntity, ReferencingEntity, CascadeDelete

# SchemaName                    ReferencedEntity ReferencingEntity CascadeDelete
# ----------                   ----------------- ----------------- -------------
# account_Account_Email_Email   account          email            Cascade
# contact_Contact_Email_Email   contact          email            Cascade
# lead_Lead_Email_Email         lead             email            Cascade

```
