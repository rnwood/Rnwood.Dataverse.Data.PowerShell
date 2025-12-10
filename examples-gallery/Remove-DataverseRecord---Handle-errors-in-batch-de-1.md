---
title: "Remove-DataverseRecord - Handle errors in batch delete operations"
tags: ['Data']
source: "Remove-DataverseRecord.md"
---
Demonstrates batch error handling for delete operations. When using batching (default BatchSize of 100), the cmdlet continues processing all records even if some deletions fail. Each error written to the error stream includes the original input object as the `TargetObject`, allowing you to correlate which record caused the error. Use `-ErrorVariable` to collect errors and `-ErrorAction SilentlyContinue` to prevent them from stopping execution.

```powershell
$recordsToDelete = Get-DataverseRecord -TableName contact -Filter @{ lastname = "TestUser" }

$errors = @()
$recordsToDelete | Remove-DataverseRecord -ErrorVariable +errors -ErrorAction SilentlyContinue

# Process any errors that occurred
foreach ($err in $errors) {
    Write-Host "Failed to delete record: $($err.TargetObject.Id)"
    Write-Host "Error: $($err.Exception.Message)"
}

```

