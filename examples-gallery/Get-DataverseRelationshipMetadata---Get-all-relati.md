---
title: "Get-DataverseRelationshipMetadata - Get all relationships for an entity"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Retrieves all relationships involving the `account` entity.

```powershell
$relationships = Get-DataverseRelationshipMetadata -EntityName account
$relationships.Count
# 45

$relationships | Select-Object -First 5 SchemaName, RelationshipType, ReferencedEntity, ReferencingEntity

# SchemaName                    RelationshipType ReferencedEntity ReferencingEntity
# ----------                   ---------------- ----------------- ------------------
# account_primary_contact       OneToMany        account          contact
# account_customer_accounts     OneToMany        account          account
# account_parent_account        OneToMany        account          account
# account_master_account        OneToMany        account          account
# account_Account_Email_Email   OneToMany        account          email

```
