---
title: "Remove-DataverseRecord - Delete large dataset with parallel processing"
tags: ['Data']
source: "Remove-DataverseRecord.md"
---
Deletes multiple contact records using parallel processing with 4 concurrent workers. Each worker processes records in batches using its own cloned connection. Parallel processing is most effective for large datasets where network latency is a bottleneck. The `-Verbose` flag shows worker task creation and progress updates.

```powershell
$recordsToDelete = Get-DataverseRecord -TableName contact -Filter @{ lastname = "TestUser" }

# Delete using 4 parallel workers for improved performance
$recordsToDelete | Remove-DataverseRecord -MaxDegreeOfParallelism 4 -Verbose

```

