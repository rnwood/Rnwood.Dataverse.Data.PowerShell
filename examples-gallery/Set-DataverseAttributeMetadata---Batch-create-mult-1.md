---
title: "Set-DataverseAttributeMetadata - Batch create multiple attributes"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates multiple attributes by iterating through a collection of attribute definitions.

```powershell
$attributes = @(
    @{ AttributeName = "new_field1"; SchemaName = "new_Field1"; DisplayName = "Field 1"; AttributeType = "String"; MaxLength = 100 }
    @{ AttributeName = "new_field2"; SchemaName = "new_Field2"; DisplayName = "Field 2"; AttributeType = "Integer"; MinValue = 0; MaxValue = 100 }
    @{ AttributeName = "new_field3"; SchemaName = "new_Field3"; DisplayName = "Field 3"; AttributeType = "Boolean"; TrueLabel = "Yes"; FalseLabel = "No" }
)

foreach ($attr in $attributes) {
    Set-DataverseAttributeMetadata -EntityName account @attr
}

```
