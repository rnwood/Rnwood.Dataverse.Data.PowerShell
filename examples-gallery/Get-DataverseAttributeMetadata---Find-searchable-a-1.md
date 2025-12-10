---
title: "Get-DataverseAttributeMetadata - Find searchable attributes"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Finds all attributes that can be used in Advanced Find.

```powershell
Get-DataverseAttributeMetadata -EntityName contact | 
    Where-Object { $_.IsValidForAdvancedFind.Value -eq $true } |
    Select-Object LogicalName, DisplayName

# LogicalName     DisplayName
# -----------    -----------
# firstname       First Name
# lastname        Last Name
# emailaddress1   Email
# jobtitle        Job Title

```
