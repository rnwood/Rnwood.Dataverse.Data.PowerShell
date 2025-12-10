---
title: "Set-DataverseAttributeMetadata - Create an image attribute"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates an image attribute for storing profile photos up to 5MB.

```powershell
Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_photo `
   -SchemaName new_Photo -DisplayName "Profile Photo" -AttributeType Image `
   -MaxSizeInKB 5120

```
