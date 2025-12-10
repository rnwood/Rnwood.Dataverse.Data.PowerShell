---
title: "Set-DataverseRecord - Batch operations with state/status changes"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Closes all active cases by setting their state and status. The cmdlet automatically handles state/status changes as separate SetStateRequest operations after the main update.

```powershell
$cases = Get-DataverseRecord -TableName incident -Filter @{ statecode = 0 }

$cases | ForEach-Object {
    $_.statuscode = "Resolved"  # Can use status label
    $_.statecode = 1             # Or numeric state value
} | Set-DataverseRecord 

```

