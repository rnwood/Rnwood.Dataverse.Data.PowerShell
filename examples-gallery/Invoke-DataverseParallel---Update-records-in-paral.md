---
title: "Invoke-DataverseParallel - Update records in parallel"
tags: ['Data']
source: "Invoke-DataverseParallel.md"
---
Updates 1000 contact records in parallel with 8 concurrent workers, processing 50 records per chunk.

```powershell
$connection = Get-DataverseConnection -url 'https://myorg.crm.dynamics.com' -ClientId $env:CLIENT_ID -ClientSecret $env:CLIENT_SECRET
Get-DataverseRecord -TableName contact -Top 1000 -Columns emailaddress1 |
  Invoke-DataverseParallel -ChunkSize 50 -MaxDegreeOfParallelism 8 -ScriptBlock {
    $_ |
       ForEach-Object{ $_.emailaddress1 = "updated-$($_.contactid)@example.com"; $_ } |
       Set-DataverseRecord -TableName contact -UpdateAllColumns
  }

```

