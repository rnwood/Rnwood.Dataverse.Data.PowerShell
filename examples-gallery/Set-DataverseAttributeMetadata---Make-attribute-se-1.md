---
title: "Set-DataverseAttributeMetadata - Make attribute searchable"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a searchable text attribute that can be used in Advanced Find queries.

```powershell
Set-DataverseAttributeMetadata -EntityName account -AttributeName new_keywords `
   -SchemaName new_Keywords -DisplayName "Keywords" -AttributeType String `
   -MaxLength 500 -IsSearchable

```
