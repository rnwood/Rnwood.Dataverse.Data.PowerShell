---
title: "Remove-DataverseEntityMetadata - Delete multiple entities"
tags: ['Metadata']
source: "Remove-DataverseEntityMetadata.md"
---
Deletes multiple entities by passing entity names through the pipeline.

```powershell
@("new_entity1", "new_entity2", "new_entity3") | Remove-DataverseEntityMetadata -Force

```
