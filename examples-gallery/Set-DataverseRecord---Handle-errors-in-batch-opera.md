---
title: "Set-DataverseRecord - Handle errors in batch operations"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Demonstrates batch error handling. When using batching (default BatchSize of 100), the cmdlet continues processing all records even if some fail. Each error written to the error stream includes the original input object as the `TargetObject`, allowing you to correlate which input caused the error. Use `-ErrorVariable` to collect errors and `-ErrorAction SilentlyContinue` to prevent them from stopping execution.

```powershell
$records = @(
    @{ firstname = "Alice"; lastname = "Valid"; emailaddress1 = "alice@example.com" }
    @{ firstname = "Bob"; lastname = ""; emailaddress1 = "bob@example.com" }  # Invalid - required field missing
    @{ firstname = "Charlie"; lastname = "Valid"; emailaddress1 = "charlie@example.com" }
)

$errors = @()
$records | Set-DataverseRecord -TableName contact -CreateOnly -ErrorVariable +errors -ErrorAction SilentlyContinue

# Process errors - each error's TargetObject contains the input record that failed
foreach ($err in $errors) {
    Write-Host "Failed to create contact: $($err.TargetObject.firstname) $($err.TargetObject.lastname)"
    Write-Host "Error: $($err.Exception.Message)"
}

```

