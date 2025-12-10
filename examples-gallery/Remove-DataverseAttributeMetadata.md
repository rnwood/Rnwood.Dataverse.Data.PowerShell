---
title: "Remove-DataverseAttributeMetadata"
tags: ['CRUD', 'Metadata']
source: "Metadata-CRUD-Examples.md"
---
Deletes attributes from entities.

```powershell
# Remove an attribute (will prompt for confirmation)
Remove-DataverseAttributeMetadata -EntityName new_customentity -AttributeName new_oldfield

# Remove with force (no confirmation)
Remove-DataverseAttributeMetadata -EntityName new_customentity -AttributeName new_oldfield -Force

# Use WhatIf to see what would happen
Remove-DataverseAttributeMetadata -EntityName new_customentity -AttributeName new_oldfield -WhatIf

```
