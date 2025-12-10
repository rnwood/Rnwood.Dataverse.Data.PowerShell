---
title: "Remove-DataverseRelationshipMetadata - Delete multiple test relationships"
tags: ['Metadata']
source: "Remove-DataverseRelationshipMetadata.md"
---
Deletes multiple relationships by passing relationship names through the pipeline.

```powershell
@("new_test_rel1", "new_test_rel2", "new_test_rel3") | 
    Remove-DataverseRelationshipMetadata -Force

```
