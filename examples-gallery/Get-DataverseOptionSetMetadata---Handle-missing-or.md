---
title: "Get-DataverseOptionSetMetadata - Handle missing or null labels"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Handles option sets with missing or null labels gracefully.

```powershell
$optionSet = Get-DataverseOptionSetMetadata -EntityName account -AttributeName industrycode
$optionSet.Options | ForEach-Object {
    $label = if ($_.Label.UserLocalizedLabel) { 
        $_.Label.UserLocalizedLabel.Label 
    } else { 
        "(No label)" 
    }
    [PSCustomObject]@{
#         Value = $_.Value
        Label = $label
    }
}

```
