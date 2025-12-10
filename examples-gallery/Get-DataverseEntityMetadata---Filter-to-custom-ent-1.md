---
title: "Get-DataverseEntityMetadata - Filter to custom entities only"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Retrieves only custom (user-created) entities.

```powershell
$customEntities = Get-DataverseEntityMetadata | Where-Object { $_.IsCustomEntity -eq $true }
$customEntities | Select-Object LogicalName, DisplayName, SchemaName

# LogicalName     DisplayName     SchemaName
# -----------    -----------    ----------
# new_project     Project         new_Project
# new_task        Task            new_Task
# new_resource    Resource        new_Resource

```
