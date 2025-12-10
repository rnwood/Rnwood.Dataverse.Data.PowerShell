---
title: "Get-DataverseEntityMetadata - Get comprehensive metadata (all options by default)"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Retrieves complete entity metadata - all details are included by default.

```powershell
# By default, all metadata is included
$metadata = Get-DataverseEntityMetadata -EntityName contact

[PSCustomObject]@{
    Entity = $metadata.LogicalName
    Attributes = $metadata.Attributes.Count
    OneToMany = $metadata.OneToManyRelationships.Count
    ManyToOne = $metadata.ManyToOneRelationships.Count
    ManyToMany = $metadata.ManyToManyRelationships.Count
    Privileges = $metadata.Privileges.Count
}

# Entity     : contact
# Attributes : 120
# OneToMany  : 38
# ManyToOne  : 15
# ManyToMany : 5
# Privileges : 8

```
