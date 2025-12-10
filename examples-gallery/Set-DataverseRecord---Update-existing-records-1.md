---
title: "Set-DataverseRecord - Update existing records"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Retrieves all existing contacts and sets their status reason to `Inactive`.

```powershell
Get-DataverseRecord -TableName contact -Columns statuscode | 
    ForEach-Object { $_.statuscode = "Inactive" } | 
    Set-DataverseRecord 

```

