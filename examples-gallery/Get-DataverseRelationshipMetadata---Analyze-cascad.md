---
title: "Get-DataverseRelationshipMetadata - Analyze cascade behaviors"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Analyzes cascade behaviors for OneToMany relationships.

```powershell
$relationships = Get-DataverseRelationshipMetadata -EntityName account -RelationshipType OneToMany
$relationships | Select-Object SchemaName, ReferencingEntity, 
    @{Name="CascadeDelete"; Expression={$_.CascadeDelete.Value}},
    @{Name="CascadeAssign"; Expression={$_.CascadeAssign.Value}},
    @{Name="CascadeShare"; Expression={$_.CascadeShare.Value}}

# SchemaName                    ReferencingEntity CascadeDelete CascadeAssign CascadeShare
# ----------                   ----------------- ------------- ------------- ------------
# account_primary_contact       contact          RemoveLink    NoCascade     NoCascade
# account_customer_accounts     account          RemoveLink    NoCascade     NoCascade
# account_parent_account        account          RemoveLink    NoCascade     NoCascade
# account_master_account        account          RemoveLink    NoCascade     NoCascade
# account_Account_Email_Email   email            Cascade       Cascade       Cascade

```
