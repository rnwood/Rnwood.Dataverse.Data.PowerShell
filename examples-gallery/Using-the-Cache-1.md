---
title: "Using the Cache"
tags: ['CRUD', 'Metadata']
source: "Metadata-CRUD-Examples.md"
---
Use the `-UseMetadataCache` parameter on Get cmdlets to utilize the shared cache:

```powershell
# Use cache for entity metadata retrieval
$metadata = Get-DataverseEntityMetadata onn -EntityName contact -UseMetadataCache

# Use cache for listing entities
$entities = Get-DataverseEntityMetadata onn -UseMetadataCache

# Use cache for attribute metadata
$attributes = Get-DataverseAttributeMetadata onn -EntityName contact -UseMetadataCache

```

