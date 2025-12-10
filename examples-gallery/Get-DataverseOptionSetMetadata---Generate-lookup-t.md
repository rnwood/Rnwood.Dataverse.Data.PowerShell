---
title: "Get-DataverseOptionSetMetadata - Generate lookup table for choice fields"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Creates a hashtable for quick value-to-label lookups.

```powershell
$optionSet = Get-DataverseOptionSetMetadata -EntityName lead -AttributeName leadqualitycode
$lookup = @{}
foreach ($option in $optionSet.Options) {
    $lookup[$option.Value] = $option.Label.UserLocalizedLabel.Label
}

# Use lookup table
$lookup[3]
# Hot

```
