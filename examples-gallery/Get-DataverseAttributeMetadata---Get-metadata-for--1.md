---
title: "Get-DataverseAttributeMetadata - Get metadata for a specific attribute"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Retrieves metadata for the `firstname` attribute on the `contact` entity.

```powershell
$attr = Get-DataverseAttributeMetadata -EntityName contact -AttributeName firstname
$attr

# LogicalName      : firstname
# SchemaName       : FirstName
# DisplayName      : First Name
# AttributeType    : String
# MaxLength        : 50
# RequiredLevel    : None
# IsSearchable     : True
# IsAuditEnabled   : True

```
