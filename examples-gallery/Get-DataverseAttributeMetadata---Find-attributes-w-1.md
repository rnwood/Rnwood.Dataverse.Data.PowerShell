---
title: "Get-DataverseAttributeMetadata - Find attributes with field-level security"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Finds attributes with field-level security enabled.

```powershell
Get-DataverseAttributeMetadata -EntityName contact | 
    Where-Object { $_.IsSecured -eq $true } |
    Select-Object LogicalName, DisplayName

# LogicalName     DisplayName
# -----------    -----------
# socialprofile   Social Profile

```
