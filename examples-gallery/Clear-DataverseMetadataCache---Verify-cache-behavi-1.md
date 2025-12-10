---
title: "Clear-DataverseMetadataCache - Verify cache behavior"
tags: ['System']
source: "Clear-DataverseMetadataCache.md"
---
Demonstrates the cache behavior and impact of clearing it.

```powershell
# First call - populates cache
Measure-Command { 
    Get-DataverseEntityMetadata -EntityName contact -UseMetadataCache 
} | Select-Object Milliseconds

# Milliseconds
# ------------
# 450

# Second call - uses cache (fast)
Measure-Command { 
    Get-DataverseEntityMetadata -EntityName contact -UseMetadataCache 
} | Select-Object Milliseconds

# Milliseconds
# ------------
# 2

# Clear cache
Clear-DataverseMetadataCache

# Third call - cache cleared, fetches again
Measure-Command { 
    Get-DataverseEntityMetadata -EntityName contact -UseMetadataCache 
} | Select-Object Milliseconds

# Milliseconds
# ------------
# 420

```
