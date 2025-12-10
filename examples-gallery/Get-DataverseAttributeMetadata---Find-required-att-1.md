---
title: "Get-DataverseAttributeMetadata - Find required attributes"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Finds all required attributes on the `account` entity.

```powershell
Get-DataverseAttributeMetadata -EntityName account | 
    Where-Object { $_.RequiredLevel.Value -in @('ApplicationRequired', 'SystemRequired') } |
    Select-Object LogicalName, DisplayName, RequiredLevel

# LogicalName     DisplayName     RequiredLevel
# -----------    -----------    -------------
# accountname     Account Name    ApplicationRequired

```
