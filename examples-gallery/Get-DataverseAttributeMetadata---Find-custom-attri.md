---
title: "Get-DataverseAttributeMetadata - Find custom attributes only"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Finds only custom (user-created) attributes.

```powershell
Get-DataverseAttributeMetadata -EntityName account | 
    Where-Object { $_.IsCustomAttribute -eq $true } |
    Select-Object LogicalName, DisplayName, AttributeType

# LogicalName         DisplayName         AttributeType
# -----------        -----------        -------------
# new_customfield     Custom Field        String
# new_priority        Priority            Picklist
# new_projectid       Project             Lookup

```
