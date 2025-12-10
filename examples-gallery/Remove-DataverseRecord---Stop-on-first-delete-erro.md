---
title: "Remove-DataverseRecord - Stop on first delete error with BatchSize 1"
tags: ['Data']
source: "Remove-DataverseRecord.md"
---
Disables batching by setting `-BatchSize 1`. With this setting, each record is deleted in a separate request, and execution stops immediately on the first error (when using `-ErrorAction Stop`). This is useful when you need to stop processing on the first failure rather than attempting to delete all records.

```powershell
$recordsToDelete = Get-DataverseRecord -TableName contact -Top 100

try {
    $recordsToDelete | Remove-DataverseRecord -BatchSize 1 -ErrorAction Stop
} catch {
    Write-Host "Error deleting record: $($_.TargetObject.Id)"
    Write-Host "Remaining records were not deleted"
}

```

