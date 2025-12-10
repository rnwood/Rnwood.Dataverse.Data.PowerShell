---
title: "Get-DataverseOptionSetMetadata - Find option by label"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Finds the numeric value for a specific option label.

```powershell
$optionSet = Get-DataverseOptionSetMetadata -EntityName contact -AttributeName preferredcontactmethodcode
$emailOption = $optionSet.Options | 
    Where-Object { $_.Label.UserLocalizedLabel.Label -eq 'Email' }
    
$emailOption.Value
# 1

```
