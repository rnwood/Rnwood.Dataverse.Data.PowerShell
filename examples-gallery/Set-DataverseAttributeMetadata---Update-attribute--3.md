---
title: "Set-DataverseAttributeMetadata - Update attribute required level"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Changes the required level of the email address field to make it application-required.

```powershell
Set-DataverseAttributeMetadata -EntityName contact -AttributeName emailaddress1 `
   -RequiredLevel ApplicationRequired

```
