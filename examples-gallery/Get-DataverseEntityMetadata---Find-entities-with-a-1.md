---
title: "Get-DataverseEntityMetadata - Find entities with audit enabled"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Finds all entities with auditing enabled.

```powershell
$auditEntities = Get-DataverseEntityMetadata | 
    Where-Object { $_.IsAuditEnabled.Value -eq $true }

$auditEntities | Select-Object LogicalName, DisplayName

# LogicalName     DisplayName
# -----------    -----------
# account         Account
# contact         Contact
# opportunity     Opportunity

```
