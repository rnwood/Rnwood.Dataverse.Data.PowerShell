---
title: "Get-DataverseOptionSetMetadata - Get default option value"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Finds the default value for a choice field.

```powershell
$attr = Get-DataverseAttributeMetadata -EntityName account -AttributeName industrycode
if ($attr.DefaultFormValue) {
    $optionSet = Get-DataverseOptionSetMetadata -EntityName account -AttributeName industrycode
    $defaultOption = $optionSet.Options | 
        Where-Object { $_.Value -eq $attr.DefaultFormValue }
    Write-Host "Default value: $($defaultOption.Label.UserLocalizedLabel.Label)"
}

```
