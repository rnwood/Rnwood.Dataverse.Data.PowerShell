---
title: "Set-DataverseAttributeMetadata - Create UniqueIdentifier attribute"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a GUID attribute for storing unique identifiers from external systems.

```powershell
Set-DataverseAttributeMetadata -EntityName account -AttributeName new_externalid `
   -SchemaName new_ExternalId -DisplayName "External ID" -AttributeType UniqueIdentifier `
   -Description "Unique identifier from external system"

```
