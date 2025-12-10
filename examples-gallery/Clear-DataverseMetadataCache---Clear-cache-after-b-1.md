---
title: "Clear-DataverseMetadataCache - Clear cache after bulk metadata changes"
tags: ['System']
source: "Clear-DataverseMetadataCache.md"
---
Clears cache after making multiple metadata changes.

```powershell
# Make multiple metadata changes
Set-DataverseAttributeMetadata -EntityName account -AttributeName new_field1 ...
Set-DataverseAttributeMetadata -EntityName account -AttributeName new_field2 ...
Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_field3 ...

# Cache is automatically invalidated per entity
# # But if you want to ensure complete cleanup:
Clear-DataverseMetadataCache

```
