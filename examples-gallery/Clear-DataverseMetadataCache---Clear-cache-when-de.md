---
title: "Clear-DataverseMetadataCache - Clear cache when debugging"
tags: ['System']
source: "Clear-DataverseMetadataCache.md"
---
Uses verbose output to see cache operations during debugging.

```powershell
# During debugging, you might want fresh data
Clear-DataverseMetadataCache -Verbose
# VERBOSE: Clearing metadata cache for all connections

$metadata = Get-DataverseEntityMetadata -EntityName account -UseMetadataCache -Verbose
# VERBOSE: Retrieving entity metadata from server (cache miss)

```
