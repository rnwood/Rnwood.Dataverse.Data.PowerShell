---
title: "Get-DataverseAttributeMetadata - Generate field documentation"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Generates comprehensive field documentation for an entity.

```powershell
$attrs = Get-DataverseAttributeMetadata -EntityName account
foreach ($attr in $attrs) {
    [PSCustomObject]@{
        Field = $attr.LogicalName
        Label = $attr.DisplayName.UserLocalizedLabel.Label
        Type = $attr.AttributeType
        Required = $attr.RequiredLevel.Value
        Description = $attr.Description.UserLocalizedLabel.Label
    }
} | Export-Csv -Path "account_fields_doc.csv" -NoTypeInformation

```
