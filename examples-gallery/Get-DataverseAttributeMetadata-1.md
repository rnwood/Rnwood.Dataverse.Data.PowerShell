---
title: "Get-DataverseAttributeMetadata"
tags: ['CRUD', 'Metadata']
source: "Metadata-CRUD-Examples.md"
---
Retrieves attribute/column metadata.

```powershell
# Get metadata for a specific attribute
Get-DataverseAttributeMetadata -EntityName contact -AttributeName firstname

# Get all attributes for an entity
Get-DataverseAttributeMetadata -EntityName contact

# Pipeline example - get all string attributes
Get-DataverseAttributeMetadata -EntityName contact | Where-Object { $_.AttributeType -eq 'String' }

# Get attributes with max length info
Get-DataverseAttributeMetadata -EntityName contact | Where-Object { $_.MaxLength } | Select-Object LogicalName, MaxLength

```
