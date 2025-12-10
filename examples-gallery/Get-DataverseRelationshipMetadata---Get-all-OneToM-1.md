---
title: "Get-DataverseRelationshipMetadata - Get all OneToMany relationships"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Retrieves all OneToMany relationships in the organization.

```powershell
$oneToMany = Get-DataverseRelationshipMetadata -RelationshipType OneToMany
$oneToMany.Count
# 234

$oneToMany | Where-Object { $_.ReferencedEntity -eq "account" } | 
    Select-Object SchemaName, ReferencingEntity, CascadeDelete

# SchemaName                    ReferencingEntity CascadeDelete
# ----------                   ----------------- -------------
# account_primary_contact       contact          RemoveLink
# account_customer_accounts     account          RemoveLink
# account_parent_account        account          RemoveLink
# account_master_account        account          RemoveLink
# account_Account_Email_Email   email            Cascade

```
