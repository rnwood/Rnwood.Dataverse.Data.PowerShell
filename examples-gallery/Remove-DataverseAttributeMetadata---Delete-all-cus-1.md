---
title: "Remove-DataverseAttributeMetadata - Delete all custom attributes from an entity"
tags: ['Metadata']
source: "Remove-DataverseAttributeMetadata.md"
---
Finds and deletes all custom attributes (starting with "new_") from an entity.

```powershell
$entity = Get-DataverseEntityMetadata -EntityName new_customentity
$entity.Attributes | 
    Where-Object { $_.LogicalName -like "new_*" } |
    ForEach-Object {
        Write-Host "Deleting attribute: $($_.LogicalName)"
        Remove-DataverseAttributeMetadata -EntityName new_customentity `
           -AttributeName $_.LogicalName -Force
    }

```
