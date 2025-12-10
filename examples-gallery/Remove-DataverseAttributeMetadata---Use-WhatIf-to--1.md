---
title: "Remove-DataverseAttributeMetadata - Use WhatIf to preview deletion"
tags: ['Metadata']
source: "Remove-DataverseAttributeMetadata.md"
---
Shows what would happen without actually deleting the attribute.

```powershell
Remove-DataverseAttributeMetadata -EntityName account -AttributeName new_testfield -WhatIf

# What if: Performing the operation "Delete attribute 'new_testfield'" on target "Entity 'account'".

```
