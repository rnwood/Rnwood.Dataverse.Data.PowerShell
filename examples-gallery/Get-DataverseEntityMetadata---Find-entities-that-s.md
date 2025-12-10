---
title: "Get-DataverseEntityMetadata - Find entities that support activities"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Finds all entities that can be associated with activities.

```powershell
$activityEntities = Get-DataverseEntityMetadata | 
    Where-Object { $_.IsActivityParty.Value -eq $true }

$activityEntities.Count
# 25

```
