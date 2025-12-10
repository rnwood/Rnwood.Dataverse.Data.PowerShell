---
title: "Get-DataverseAttributeMetadata - Export attribute list to CSV"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Exports attribute metadata to CSV for documentation.

```powershell
Get-DataverseAttributeMetadata -EntityName account | 
    Select-Object LogicalName, DisplayName, AttributeType, RequiredLevel, MaxLength | 
    Export-Csv -Path "account_attributes.csv" -NoTypeInformation

```
