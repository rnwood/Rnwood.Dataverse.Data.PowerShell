---
title: "Remove-DataverseRecord - Combine parallel processing with batching for maximum throughput"
tags: ['Data']
source: "Remove-DataverseRecord.md"
---
Combines parallel processing with large batch sizes for maximum throughput when deleting thousands of records. Uses 8 parallel workers, each processing records in batches of 200. This configuration is optimal for very large delete operations where both network latency and API throughput are concerns.

```powershell
# Get large dataset of records to delete
$recordsToDelete = Get-DataverseRecord -TableName account -Filter @{ statuscode = 2 }

# Delete using 8 parallel workers, each using batch size of 200
$recordsToDelete | Remove-DataverseRecord -MaxDegreeOfParallelism 8 -BatchSize 200 -Verbose

```

