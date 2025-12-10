---
title: "Set-DataverseRecord - Using retry logic for transient failures"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Creates multiple contacts with automatic retry on transient failures. Failed requests will be retried up to 3 times with delays of 500ms, 1000ms, and 2000ms respectively. The `-Verbose` flag shows retry attempts and wait times.

```powershell
$records = @(
    @{ firstname = "Alice"; lastname = "Smith" }
    @{ firstname = "Bob"; lastname = "Jones" }
    @{ firstname = "Carol"; lastname = "Williams" }
)

# Retry each failed record up to 3 times with exponential backoff
$records | Set-DataverseRecord -TableName contact -CreateOnly -Retries 3 -InitialRetryDelay 500 -Verbose

```

