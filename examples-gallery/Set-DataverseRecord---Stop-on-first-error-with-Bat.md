---
title: "Set-DataverseRecord - Stop on first error with BatchSize 1"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Disables batching by setting `-BatchSize 1`. With this setting, each record is sent in a separate request, and execution stops immediately on the first error (when using `-ErrorAction Stop`). This is useful when you need to stop processing on the first failure rather than attempting all records.

```powershell
$records = @(
    @{ firstname = "Alice"; lastname = "Valid" }
    @{ firstname = "Bob"; lastname = "" }  # Invalid - will cause error
    @{ firstname = "Charlie"; lastname = "Valid" }  # Won't be processed
)

try {
    $records | Set-DataverseRecord -TableName contact -CreateOnly -BatchSize 1 -ErrorAction Stop
} catch {
    Write-Host "Error creating record: $($_.TargetObject.firstname)"
    Write-Host "Remaining records were not processed"
}

```

