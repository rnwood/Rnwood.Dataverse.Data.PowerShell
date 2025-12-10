---
title: "Get-DataverseRelationshipMetadata - Get relationship statistics"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Generates statistics about relationships in the organization.

```powershell
$allRelationships = Get-DataverseRelationshipMetadata
$stats = [PSCustomObject]@{
    TotalRelationships = $allRelationships.Count
    OneToMany          = ($allRelationships | Where-Object { $_.RelationshipType -eq "OneToMany" }).Count
    ManyToOne          = ($allRelationships | Where-Object { $_.RelationshipType -eq "ManyToOne" }).Count
    ManyToMany         = ($allRelationships | Where-Object { $_.RelationshipType -eq "ManyToMany" }).Count
    CustomRelationships = ($allRelationships | Where-Object { $_.IsCustomRelationship }).Count
    SystemRelationships = ($allRelationships | Where-Object { -not $_.IsCustomRelationship }).Count
}

$stats

# TotalRelationships  : 456
# OneToMany          : 234
# ManyToOne          : 189
# ManyToMany         : 33
# CustomRelationships : 12
# SystemRelationships : 444

```
