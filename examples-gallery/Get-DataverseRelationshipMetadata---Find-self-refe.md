---
title: "Get-DataverseRelationshipMetadata - Find self-referencing relationships"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Finds hierarchical (self-referencing) relationships.

```powershell
$selfReferencing = Get-DataverseRelationshipMetadata | 
    Where-Object { $_.ReferencedEntity -eq $_.ReferencingEntity -and $_.RelationshipType -eq "OneToMany" }

$selfReferencing | Select-Object SchemaName, ReferencedEntity, 
    @{Name="IsHierarchical"; Expression={$_.IsHierarchical}}

# SchemaName                    ReferencedEntity IsHierarchical
# ----------                   ----------------- --------------
# account_parent_account        account          True
# contact_parent_contact        contact          True
# systemuser_parent_systemuser  systemuser       True

```
