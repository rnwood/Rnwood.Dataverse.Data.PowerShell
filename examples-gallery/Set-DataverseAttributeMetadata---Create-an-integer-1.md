---
title: "Set-DataverseAttributeMetadata - Create an integer attribute with constraints"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates an integer attribute with minimum value of 0 and maximum of 10000, making it required by the application.

```powershell
Set-DataverseAttributeMetadata -EntityName product -AttributeName new_quantity `
   -SchemaName new_Quantity -DisplayName "Quantity" -AttributeType Integer `
   -MinValue 0 -MaxValue 10000 -RequiredLevel ApplicationRequired

```
