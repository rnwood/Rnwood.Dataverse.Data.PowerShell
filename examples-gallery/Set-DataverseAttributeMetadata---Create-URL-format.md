---
title: "Set-DataverseAttributeMetadata - Create URL format string attribute"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a text attribute with URL format, displayed as a clickable link.

```powershell
Set-DataverseAttributeMetadata -EntityName account -AttributeName new_website2 `
   -SchemaName new_Website2 -DisplayName "Secondary Website" -AttributeType String `
   -MaxLength 200 -StringFormat Url

```
