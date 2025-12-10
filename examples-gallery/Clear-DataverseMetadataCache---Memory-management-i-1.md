---
title: "Clear-DataverseMetadataCache - Memory management in long-running scripts"
tags: ['System']
source: "Clear-DataverseMetadataCache.md"
---
Manages memory in long-running scripts processing many entities.

```powershell
# Process many entities
foreach ($entity in $entityList) {
    # Get metadata with cache
    $metadata = Get-DataverseEntityMetadata -EntityName $entity -UseMetadataCache
    
    # Process metadata...
    
    # Clear cache every 100 entities to manage memory
    if ($processedCount % 100 -eq 0) {
        Clear-DataverseMetadataCache
    }
}

```
