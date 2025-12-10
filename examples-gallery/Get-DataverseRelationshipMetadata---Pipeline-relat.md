---
title: "Get-DataverseRelationshipMetadata - Pipeline relationship names"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Processes multiple relationship names through the pipeline.

```powershell
@("account_primary_contact", "contact_parent_contact", "lead_qualifying_lead") | 
    Get-DataverseRelationshipMetadata | 
    Select-Object SchemaName, RelationshipType, ReferencedEntity, ReferencingEntity

# SchemaName                    RelationshipType ReferencedEntity ReferencingEntity
# ----------                   ---------------- ----------------- ------------------
# account_primary_contact       OneToMany        account          contact
# contact_parent_contact        OneToMany        contact          contact
# lead_qualifying_lead          OneToMany        lead             lead

```
