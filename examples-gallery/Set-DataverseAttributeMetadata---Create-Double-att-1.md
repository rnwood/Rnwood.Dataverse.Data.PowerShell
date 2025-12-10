---
title: "Set-DataverseAttributeMetadata - Create Double attribute"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a double-precision floating point attribute with 4 decimal places.

```powershell
Set-DataverseAttributeMetadata -EntityName measurement -AttributeName new_temperature `
   -SchemaName new_Temperature -DisplayName "Temperature" -AttributeType Double `
   -MinValue -273.15 -MaxValue 1000 -Precision 4

```
