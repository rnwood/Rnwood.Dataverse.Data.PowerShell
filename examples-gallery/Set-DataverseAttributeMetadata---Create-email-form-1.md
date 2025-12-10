---
title: "Set-DataverseAttributeMetadata - Create email format string attribute"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a text attribute with email format validation.

```powershell
Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_secondaryemail `
   -SchemaName new_SecondaryEmail -DisplayName "Secondary Email" -AttributeType String `
   -MaxLength 100 -StringFormat Email

```
