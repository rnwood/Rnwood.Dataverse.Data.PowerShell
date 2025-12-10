---
title: "Set-DataverseAttributeMetadata - Create phone format string attribute"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a text attribute with phone number formatting.

```powershell
Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_mobilephone2 `
   -SchemaName new_MobilePhone2 -DisplayName "Secondary Mobile" -AttributeType String `
   -MaxLength 20 -StringFormat Phone

```
