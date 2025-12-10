---
title: "Get-DataverseEntityMetadata - Use metadata cache for performance"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Demonstrates the performance improvement when using the metadata cache.

```powershell
# First call - fetches from server
Measure-Command { $metadata1 = Get-DataverseEntityMetadata -EntityName contact -UseMetadataCache }

# Milliseconds : 450

# Second call - uses cache
Measure-Command { $metadata2 = Get-DataverseEntityMetadata -EntityName contact -UseMetadataCache }

# Milliseconds : 2

```
