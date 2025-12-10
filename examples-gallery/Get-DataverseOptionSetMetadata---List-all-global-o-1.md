---
title: "Get-DataverseOptionSetMetadata - List all global option sets"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Retrieves all global option sets in the organization.

```powershell
$allGlobalOptionSets = Get-DataverseOptionSetMetadata
$allGlobalOptionSets | Select-Object -First 10 Name, DisplayName

# Name                DisplayName
# ----               -----------
# new_customerstatus  Customer Status
# new_priority        Priority Levels
# new_category        Product Categories

```
