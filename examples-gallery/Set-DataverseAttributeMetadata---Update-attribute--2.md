---
title: "Set-DataverseAttributeMetadata - Update attribute display name and description"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Updates the display name and description of an existing attribute. No other properties are changed.

```powershell
Set-DataverseAttributeMetadata -EntityName account -AttributeName new_customfield `
   -DisplayName "Updated Field Name" -Description "Updated description for this field"

```
