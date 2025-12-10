---
title: "Remove-DataverseRecord - Using retry logic for transient delete failures"
tags: ['Data']
source: "Remove-DataverseRecord.md"
---
Deletes multiple contacts with automatic retry on transient failures. Failed deletion requests will be retried up to 3 times with delays of 500ms, 1000ms, and 2000ms respectively. The `-Verbose` flag shows retry attempts and wait times.

```powershell
$recordsToDelete = Get-DataverseRecord -TableName contact -Filter @{ lastname = "TestUser" }

# Retry each failed delete up to 3 times with exponential backoff
$recordsToDelete | Remove-DataverseRecord -Retries 3 -InitialRetryDelay 500 -Verbose

```

