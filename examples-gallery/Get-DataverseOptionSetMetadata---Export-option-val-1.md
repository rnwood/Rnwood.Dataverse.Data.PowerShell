---
title: "Get-DataverseOptionSetMetadata - Export option values to CSV"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Exports option set values to CSV for documentation or import.

```powershell
$optionSet = Get-DataverseOptionSetMetadata -EntityName account -AttributeName industrycode
$optionSet.Options | 
    Select-Object Value, @{N='Label';E={$_.Label.UserLocalizedLabel.Label}} |
    Export-Csv -Path "industry_codes.csv" -NoTypeInformation

```
