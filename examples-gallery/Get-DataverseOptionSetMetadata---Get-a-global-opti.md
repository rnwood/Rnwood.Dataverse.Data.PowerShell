---
title: "Get-DataverseOptionSetMetadata - Get a global option set by name"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Retrieves a global option set that can be reused across entities.

```powershell
$globalOptionSet = Get-DataverseOptionSetMetadata -Name new_customerstatus
$globalOptionSet.Options | Select-Object Value, Label

# Value Label
# ----- -----
# 1     Active
# 2     Inactive
# 3     Pending

```
