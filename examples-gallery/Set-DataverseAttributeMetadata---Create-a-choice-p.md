---
title: "Set-DataverseAttributeMetadata - Create a choice (picklist) attribute with local options"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a choice attribute with local (entity-specific) options.

```powershell
$options = @(
    @{ Value = 1; Label = "Small" }
    @{ Value = 2; Label = "Medium" }
    @{ Value = 3; Label = "Large" }
    @{ Value = 4; Label = "Extra Large" }
)

Set-DataverseAttributeMetadata -EntityName product -AttributeName new_size `
   -SchemaName new_Size -DisplayName "Product Size" -AttributeType Picklist `
   -Options $options

```
