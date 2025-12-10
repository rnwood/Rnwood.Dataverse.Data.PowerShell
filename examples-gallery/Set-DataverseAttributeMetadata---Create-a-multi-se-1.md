---
title: "Set-DataverseAttributeMetadata - Create a multi-select choice attribute"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a multi-select choice attribute allowing multiple values to be selected.

```powershell
$interests = @(
    @{ Value = 1; Label = "Technology" }
    @{ Value = 2; Label = "Sports" }
    @{ Value = 3; Label = "Music" }
    @{ Value = 4; Label = "Travel" }
)

Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_interests `
   -SchemaName new_Interests -DisplayName "Interests" -AttributeType MultiSelectPicklist `
   -Options $interests

```
