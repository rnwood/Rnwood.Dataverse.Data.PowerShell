---
title: "Set-DataverseAttributeMetadata - Create a multiline text attribute"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a multiline text (memo) attribute on the `contact` table with 4000 character limit and recommended requirement level.

```powershell
Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_notes `
   -SchemaName new_Notes -DisplayName "Notes" -AttributeType Memo `
   -MaxLength 4000 -RequiredLevel Recommended

```
