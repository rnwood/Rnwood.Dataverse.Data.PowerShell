---
title: "Set-DataverseAttributeMetadata - Create a file attribute"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a file attribute that can store documents up to 10MB (10240 KB).

```powershell
Set-DataverseAttributeMetadata -EntityName account -AttributeName new_contract `
   -SchemaName new_Contract -DisplayName "Contract Document" -AttributeType File `
   -MaxSizeInKB 10240

```
