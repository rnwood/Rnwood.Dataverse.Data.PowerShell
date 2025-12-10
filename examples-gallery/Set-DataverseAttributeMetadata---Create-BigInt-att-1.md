---
title: "Set-DataverseAttributeMetadata - Create BigInt attribute"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a BigInt attribute for storing large integer values beyond the range of regular integers.

```powershell
Set-DataverseAttributeMetadata -EntityName account -AttributeName new_largeid `
   -SchemaName new_LargeId -DisplayName "Large ID" -AttributeType BigInt `
   -Description "Stores very large integer values"

```
