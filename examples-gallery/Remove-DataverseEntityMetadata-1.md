---
title: "Remove-DataverseEntityMetadata"
tags: ['CRUD', 'Metadata']
source: "Metadata-CRUD-Examples.md"
---
Deletes entities.

```powershell
# Remove an entity (will prompt for confirmation)
Remove-DataverseEntityMetadata -EntityName new_oldentity

# Remove with force (no confirmation)
Remove-DataverseEntityMetadata -EntityName new_oldentity -Force

# Use WhatIf to see what would happen
Remove-DataverseEntityMetadata -EntityName new_oldentity -WhatIf

```
