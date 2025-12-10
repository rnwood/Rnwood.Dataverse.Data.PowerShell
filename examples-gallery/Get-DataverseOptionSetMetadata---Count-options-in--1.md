---
title: "Get-DataverseOptionSetMetadata - Count options in a choice field"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Counts the number of options in a choice field.

```powershell
$optionSet = Get-DataverseOptionSetMetadata -EntityName account -AttributeName accountcategorycode
Write-Host "Number of categories: $($optionSet.Options.Count)"
# Number of categories: 5

```
