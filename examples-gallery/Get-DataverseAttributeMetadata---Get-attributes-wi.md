---
title: "Get-DataverseAttributeMetadata - Get attributes with max length"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Finds string and memo attributes with their maximum lengths.

```powershell
Get-DataverseAttributeMetadata -EntityName contact | 
    Where-Object { $_.MaxLength -ne $null } |
    Select-Object LogicalName, AttributeType, MaxLength | 
    Sort-Object MaxLength -Descending

# LogicalName       AttributeType MaxLength
# -----------      ------------- ---------
# description       Memo          2000
# address1_composite String       1000
# fullname          String       160
# emailaddress1     String       100

```
