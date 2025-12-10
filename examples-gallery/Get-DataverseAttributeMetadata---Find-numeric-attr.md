---
title: "Get-DataverseAttributeMetadata - Find numeric attributes with constraints"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Finds numeric attributes with their constraints.

```powershell
Get-DataverseAttributeMetadata -EntityName account | 
    Where-Object { $_.AttributeType -in @('Integer', 'Decimal', 'Money') } |
    Select-Object LogicalName, AttributeType, MinValue, MaxValue, Precision

# LogicalName         AttributeType MinValue MaxValue Precision
# -----------        ------------- -------- -------- ---------
# numberofemployees   Integer       0        1000000000
# revenue             Money         0        100000000000 2

```
