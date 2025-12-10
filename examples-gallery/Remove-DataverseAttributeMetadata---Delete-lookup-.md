---
title: "Remove-DataverseAttributeMetadata - Delete lookup attribute"
tags: ['Metadata']
source: "Remove-DataverseAttributeMetadata.md"
---
Deletes a lookup attribute. Note that this also removes the associated relationship.

```powershell
# Note: Deleting a lookup attribute also deletes the relationship
Remove-DataverseAttributeMetadata -EntityName contact -AttributeName new_projectid -Force

```
