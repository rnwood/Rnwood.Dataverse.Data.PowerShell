---
title: "Set-DataverseAttributeMetadata - Create a money attribute"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a money attribute for bonus amounts with currency formatting.

```powershell
Set-DataverseAttributeMetadata -EntityName opportunity -AttributeName new_bonus `
   -SchemaName new_Bonus -DisplayName "Bonus Amount" -AttributeType Money `
   -MinValue 0 -MaxValue 1000000 -Precision 2

```
