---
title: "Get-DataverseEntityMetadata - Get entities with change tracking enabled"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Finds entities with change tracking enabled for data synchronization.

```powershell
Get-DataverseEntityMetadata | 
    Where-Object { $_.ChangeTrackingEnabled -eq $true } |
    Select-Object LogicalName, DisplayName

# LogicalName     DisplayName
# -----------    -----------
# account         Account
# contact         Contact

```
