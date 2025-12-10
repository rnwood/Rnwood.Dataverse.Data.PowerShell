---
title: "Get-DataverseAttributeMetadata - Find date/time attributes"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Finds all date/time attributes with their format settings.

```powershell
Get-DataverseAttributeMetadata -EntityName contact | 
    Where-Object { $_.AttributeType -eq 'DateTime' } |
    Select-Object LogicalName, DisplayName, DateTimeBehavior, Format

# LogicalName     DisplayName     DateTimeBehavior Format
# -----------    -----------    ---------------- ------
# createdon       Created On      UserLocal        DateAndTime
# modifiedon      Modified On     UserLocal        DateAndTime
# birthdate       Birthday        DateOnly         DateOnly

```
