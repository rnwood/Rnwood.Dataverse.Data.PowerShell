---
title: "Get-DataverseOptionSetMetadata - Validate choice value exists"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Validates if a choice value exists in the option set.

```powershell
$optionSet = Get-DataverseOptionSetMetadata -EntityName account -AttributeName industrycode
$valueToCheck = 99
$validValues = $optionSet.Options.Value

if ($valueToCheck -in $validValues) {
    Write-Host "Value $valueToCheck is valid"
} else {
    Write-Host "Value $valueToCheck is NOT valid"
}

```
