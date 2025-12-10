---
title: "Get-DataverseEntityMetadata - Export entity list to CSV"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Exports a list of all entities with key properties to CSV for documentation.

```powershell
Get-DataverseEntityMetadata | 
    Select-Object LogicalName, DisplayName, IsCustomEntity, OwnershipType, IsAuditEnabled | 
    Export-Csv -Path "entities.csv" -NoTypeInformation

```
