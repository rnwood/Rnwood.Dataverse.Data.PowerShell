---
title: "Get-DataverseEntityMetadata - Get metadata with relationships excluded"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Gets entity metadata without relationships, then shows the default behavior includes them.

```powershell
$metadata = Get-DataverseEntityMetadata -EntityName account -ExcludeRelationships
$metadata.OneToManyRelationships
# # Relationships will be null or minimal when excluded

# By default, relationships are included
$metadata = Get-DataverseEntityMetadata -EntityName account
$metadata.OneToManyRelationships.Count
# 45

```
