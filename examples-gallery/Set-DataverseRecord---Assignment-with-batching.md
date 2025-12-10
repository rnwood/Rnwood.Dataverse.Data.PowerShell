---
title: "Set-DataverseRecord - Assignment with batching"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Reassigns 100 accounts to a new owner. The cmdlet handles this as update operations followed by AssignRequest operations, all batched together.

```powershell
$newOwnerId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"

Get-DataverseRecord -TableName account -Top 100 | 
    ForEach-Object { $_.ownerid = $newOwnerId } | 
    Set-DataverseRecord 

```

