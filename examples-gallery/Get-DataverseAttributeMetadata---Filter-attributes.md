---
title: "Get-DataverseAttributeMetadata - Filter attributes by type"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Finds all string (text) attributes on the `contact` entity.

```powershell
Get-DataverseAttributeMetadata -EntityName contact | 
    Where-Object { $_.AttributeType -eq 'String' } |
    Select-Object LogicalName, MaxLength

# LogicalName       MaxLength
# -----------      ---------
# firstname         50
# lastname          50
# emailaddress1     100
# jobtitle          100
# department        100

```
