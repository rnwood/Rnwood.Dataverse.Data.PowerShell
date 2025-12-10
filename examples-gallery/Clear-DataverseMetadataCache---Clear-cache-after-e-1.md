---
title: "Clear-DataverseMetadataCache - Clear cache after external changes"
tags: ['System']
source: "Clear-DataverseMetadataCache.md"
---
Clears the cache after making changes through another tool.

```powershell
# You made changes in Power Apps maker portal
# Now clear the cache to see the changes
Clear-DataverseMetadataCache

# Fetch fresh metadata
$metadata = Get-DataverseEntityMetadata -EntityName account -UseMetadataCache

```
