---
title: "Get-DataverseRelationshipMetadata - Query only published metadata"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Demonstrates retrieving only published relationship metadata vs unpublished (draft) metadata. Use -Published to query only relationships that have been published.

```powershell
# Get only published relationships
$publishedRels = Get-DataverseRelationshipMetadata -EntityName account -Published
$publishedRels.Count
# 42

# Default behavior includes unpublished (draft) relationships
$unpublishedRels = Get-DataverseRelationshipMetadata -EntityName account
$unpublishedRels.Count
# 45

```
