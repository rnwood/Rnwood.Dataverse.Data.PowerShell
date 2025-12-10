---
title: "Clear-DataverseMetadataCache - Clear cache in a script"
tags: ['System']
source: "Clear-DataverseMetadataCache.md"
---
Ensures a script starts with a clean cache for reliable results.

```powershell
# Clear cache at start of script to ensure fresh data
Clear-DataverseMetadataCache

# Now proceed with metadata operations
$entities = Get-DataverseEntityMetadata -UseMetadataCache

```
