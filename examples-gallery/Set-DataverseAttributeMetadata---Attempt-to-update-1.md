---
title: "Set-DataverseAttributeMetadata - Attempt to update immutable property (will fail)"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Demonstrates that attempting to change an immutable property will result in an error.

```powershell
Set-DataverseAttributeMetadata -EntityName account -AttributeName new_customfield `
   -StringFormat Email

Set-DataverseAttributeMetadata : Cannot change StringFormat from 'Text' to 'Email'. This property is immutable after creation.

```
