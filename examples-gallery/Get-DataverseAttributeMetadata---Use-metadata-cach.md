---
title: "Get-DataverseAttributeMetadata - Use metadata cache for performance"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Demonstrates the performance improvement when using the metadata cache.

```powershell
# First call - fetches from server
Measure-Command { 
    $attrs1 = Get-DataverseAttributeMetadata -EntityName contact -UseMetadataCache 
}

# Milliseconds : 380

# Second call - uses cache
Measure-Command { 
    $attrs2 = Get-DataverseAttributeMetadata -EntityName contact -UseMetadataCache 
}

# Milliseconds : 1

```
