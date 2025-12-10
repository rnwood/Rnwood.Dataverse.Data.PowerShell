---
title: "Get-DataverseOptionSetMetadata - Get all options with descriptions"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Retrieves options with descriptions for the industry code field.

```powershell
$optionSet = Get-DataverseOptionSetMetadata -EntityName lead -AttributeName industrycode
$optionSet.Options | ForEach-Object {
    [PSCustomObject]@{
#         Value = $_.Value
        Label = $_.Label.UserLocalizedLabel.Label
        Description = $_.Description.UserLocalizedLabel.Label
    }
}

# Value Label              Description
# ----- -----             -----------
# 1     Agriculture        Agricultural and farming industry
# 2     Banking            Financial services and banking
# 3     Construction       Construction and building

```
