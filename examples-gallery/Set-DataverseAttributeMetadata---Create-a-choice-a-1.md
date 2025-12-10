---
title: "Set-DataverseAttributeMetadata - Create a choice attribute using a global option set"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a choice attribute that uses an existing global option set named `new_customerstatus`.

```powershell
Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_status `
   -SchemaName new_Status -DisplayName "Customer Status" -AttributeType Picklist `
   -OptionSetName new_customerstatus

```
