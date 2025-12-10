---
title: "Set-DataverseRecord - Batch update with choice columns"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Retrieves 50 accounts and updates their industry (using label) and rating (using numeric value). Choice/option set columns accept either the numeric value or the display label.

```powershell
$accounts = Get-DataverseRecord -TableName account -Top 50

$accounts | ForEach-Object { 
    $_.industrycode = "Retail"  # Can use label name
    $_.accountratingcode = 1     # Or numeric value
} | Set-DataverseRecord 

```

