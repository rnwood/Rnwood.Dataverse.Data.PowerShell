---
title: "Get-DataverseOptionSetMetadata - Generate documentation for all choice fields"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Generates documentation for all choice fields on an entity.

```powershell
$attributes = Get-DataverseAttributeMetadata -EntityName account
$choiceFields = $attributes | 
    Where-Object { $_.AttributeType -in @('Picklist', 'State', 'Status') }

foreach ($attr in $choiceFields) {
    Write-Host "\n$($attr.DisplayName.UserLocalizedLabel.Label):"
    $optionSet = Get-DataverseOptionSetMetadata -EntityName account -AttributeName $attr.LogicalName
    $optionSet.Options | ForEach-Object {
        Write-Host "  $($_.Value): $($_.Label.UserLocalizedLabel.Label)"
    }
}

```
