---
title: "Cache Invalidation"
tags: ['CRUD', 'Metadata']
source: "Metadata-CRUD-Examples.md"
---
The cache is automatically invalidated when you make changes using Set or Remove cmdlets:

```powershell
# This will automatically invalidate the cache for the 'new_project' entity
Set-DataverseAttributeMetadata `
   -EntityName new_project `
   -AttributeName new_field `
   -AttributeType String `
   -DisplayName "New Field"

# Next retrieval will fetch fresh data
$metadata = Get-DataverseEntityMetadata -EntityName new_project -UseMetadataCache

```
