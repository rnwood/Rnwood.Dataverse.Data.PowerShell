---
title: "Set-DataverseAttributeMetadata - Create a decimal attribute with precision"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a decimal attribute for discount percentages with 2 decimal places of precision.

```powershell
Set-DataverseAttributeMetadata -EntityName invoice -AttributeName new_discount `
   -SchemaName new_Discount -DisplayName "Discount Percentage" -AttributeType Decimal `
   -MinValue 0 -MaxValue 100 -Precision 2

```
