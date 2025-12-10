---
title: "Get-DataverseAttributeMetadata - Find lookup attributes"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Finds all lookup attributes and their target entities.

```powershell
Get-DataverseAttributeMetadata -EntityName contact | 
    Where-Object { $_.AttributeType -eq 'Lookup' } |
    Select-Object LogicalName, DisplayName, Targets

# LogicalName     DisplayName     Targets
# -----------    -----------    -------
# parentcustomerid Parent Customer {account, contact}
# ownerid         Owner           {systemuser, team}

```
