---
title: "Set-DataverseAttributeMetadata - Use -WhatIf to preview changes"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Uses -WhatIf to see what would happen without actually creating the attribute.

```powershell
Set-DataverseAttributeMetadata -EntityName account -AttributeName new_testfield `
   -SchemaName new_TestField -DisplayName "Test Field" -AttributeType String `
   -MaxLength 100 -WhatIf

# What if: Performing the operation "Create attribute 'new_TestField' of type 'String'" on target "Entity 'account'".

```
