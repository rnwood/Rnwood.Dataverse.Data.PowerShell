---
title: "Get-DataverseEntityMetadata - Find entities by display name pattern"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Finds entities with "Project" in their display name.

```powershell
Get-DataverseEntityMetadata | 
    Where-Object { $_.DisplayName.UserLocalizedLabel.Label -like "*Project*" } |
    Select-Object LogicalName, DisplayName

# LogicalName     DisplayName
# -----------    -----------
# new_project     Project
# new_projecttask Project Task

```
