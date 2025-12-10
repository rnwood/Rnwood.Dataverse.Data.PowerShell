---
title: "Get-DataverseEntityMetadata - List all entities in the organization"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Retrieves basic metadata for all entities in the organization.

```powershell
$allEntities = Get-DataverseEntityMetadata
$allEntities.Count
# 450

$allEntities | Select-Object -First 10 LogicalName, DisplayName, IsCustomEntity

# LogicalName          DisplayName          IsCustomEntity
# -----------         -----------         --------------
# account              Account              False
# contact              Contact              False
# lead                 Lead                 False
# opportunity          Opportunity          False
# new_project          Project              True
# new_task             Task                 True

```
