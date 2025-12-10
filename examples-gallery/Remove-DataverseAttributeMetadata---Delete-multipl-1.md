---
title: "Remove-DataverseAttributeMetadata - Delete multiple attributes from an entity"
tags: ['Metadata']
source: "Remove-DataverseAttributeMetadata.md"
---
Deletes multiple attributes from the `account` entity.

```powershell
@("new_field1", "new_field2", "new_field3") | 
    Remove-DataverseAttributeMetadata -EntityName account -Force

```
