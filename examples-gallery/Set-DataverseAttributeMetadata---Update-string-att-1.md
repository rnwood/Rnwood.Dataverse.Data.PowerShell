---
title: "Set-DataverseAttributeMetadata - Update string attribute maximum length"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Increases the maximum length of the account number field to 50 characters.

```powershell
Set-DataverseAttributeMetadata -EntityName account -AttributeName accountnumber `
   -MaxLength 50

```
