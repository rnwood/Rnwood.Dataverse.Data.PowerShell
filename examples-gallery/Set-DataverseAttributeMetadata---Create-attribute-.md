---
title: "Set-DataverseAttributeMetadata - Create attribute with -PassThru to see result"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates an attribute and returns the metadata of the created attribute using `-PassThru`.

```powershell
$result = Set-DataverseAttributeMetadata -EntityName account -AttributeName new_score `
   -SchemaName new_Score -DisplayName "Score" -AttributeType Integer `
   -PassThru

$result

# LogicalName : new_score
# SchemaName  : new_Score
# DisplayName : Score
# AttributeType : Integer
# MetadataId  : a1234567-89ab-cdef-0123-456789abcdef

```
