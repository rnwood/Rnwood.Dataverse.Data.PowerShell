---
title: "Set-DataverseRecord - Process records in parallel for maximum throughput"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Processes 10,000 records using 4 parallel worker threads. Each worker maintains its own batch of 100 records. This results in multiple ExecuteMultipleRequest operations running concurrently for maximum throughput. The `-Verbose` flag shows worker activity and progress across all threads.

```powershell
# Create 10,000 records with 4 parallel workers and batches of 100
$records = 1..10000 | ForEach-Object {
    @{ 
        firstname = "User$_"
        lastname = "Parallel"
        emailaddress1 = "user$_@example.com" 
    }
}

$records | Set-DataverseRecord -TableName contact -CreateOnly -MaxDegreeOfParallelism 4 -BatchSize 100 -Verbose

```

