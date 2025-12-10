---
title: "Set-DataverseRecord - Control batch size"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Creates 500 records in batches of 50. This will result in 10 ExecuteMultipleRequest calls, each containing 50 CreateRequest operations.

```powershell
$records = 1..500 | ForEach-Object {
    @{ name = "Account $_"; telephone1 = "555-$_" }
}

$records | Set-DataverseRecord -TableName account -BatchSize 50 -CreateOnly

```

