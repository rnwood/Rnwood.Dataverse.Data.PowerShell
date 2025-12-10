---
title: "Set-DataverseAttributeMetadata - Create a simple text attribute"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a new single-line text attribute named `new_customfield` on the `account` table with a maximum length of 200 characters.

```powershell
Set-DataverseAttributeMetadata -EntityName account -AttributeName new_customfield `
   -SchemaName new_CustomField -DisplayName "Custom Field" -AttributeType String `
   -MaxLength 200 -Description "A custom text field"

```
