---
title: "Remove-DataverseAttributeMetadata - Delete an attribute without confirmation"
tags: ['Metadata']
source: "Remove-DataverseAttributeMetadata.md"
---
Deletes the attribute `new_oldfield` from the `contact` entity without prompting.

```powershell
Remove-DataverseAttributeMetadata -EntityName contact -AttributeName new_oldfield -Force

```
