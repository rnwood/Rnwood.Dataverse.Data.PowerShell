---
title: "Set-DataverseAttributeMetadata - Update numeric attribute constraints"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Updates the minimum and maximum values for an existing integer attribute.

```powershell
Set-DataverseAttributeMetadata -EntityName product -AttributeName new_quantity `
   -MinValue 10 -MaxValue 5000

```
