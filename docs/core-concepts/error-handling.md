
# Error Handling and Batch Operations



When working with batch operations, errors don't stop processing - all records are attempted and errors are collected. You can correlate errors back to the specific input records that failed.

*Example: Handle errors in batch operations:*
```powershell
$records = @(
    @{ firstname = "John"; lastname = "Doe" }
    @{ firstname = "Jane"; lastname = "Smith" }
)
$errors = @()
$records | Set-DataverseRecord -Connection $c -TableName contact -CreateOnly `
    -ErrorVariable +errors -ErrorAction SilentlyContinue
# Process errors - each error's TargetObject contains the input that failed
foreach ($err in $errors) {
    Write-Host "Failed: $($err.TargetObject.firstname) $($err.TargetObject.lastname)"
    Write-Host "Error: $($err.Exception.Message)"
}
```

The `Exception.Message` contains full server response including ErrorCode, Message, TraceText, and InnerFault details for troubleshooting.

To stop on first error instead, use `-BatchSize 1` with `-ErrorAction Stop`.






Example: Retry failed operations up to 3 times with 15s initial delay:
```powershell
$records | Set-DataverseRecord -Connection $c -TableName contact -Retries 3 -InitialRetryDelay 15 -Verbose
```

**Drawbacks and Considerations:**

While retry logic improves resilience, it may not be appropriate for all operations:

- **Operations with side effects**: Some operations cannot be safely retried if they have already partially succeeded. For example, creating records might result in duplicates if the initial request succeeded but the response was lost.
- **Idempotent operations**: Retries are safest with idempotent operations (those that can be repeated without changing the result). Reading data (`Get-DataverseRecord`) and updating existing records are typically safe to retry.
- **Default behavior for Set-DataverseRecord**: The default mode performs existence checks before operations, making updates and upserts generally safe to retry. However, create-only operations (`-CreateOnly`) should be used cautiously with retries as they may create duplicate records on failure.

For operations that cannot be safely retried, consider using smaller batch sizes (`-BatchSize 1`) or handling errors explicitly rather than relying on automatic retries.

### Parallelising work for best performance

When processing many records you can use parallelism to reduce elapsed time. Use parallelism when network latency or per-request processing dominates total time, but be careful to avoid overwhelming the Dataverse service (throttling).

**For single-step operations (create/update/delete):** Use the built-in `-MaxDegreeOfParallelism` parameter on `Set-DataverseRecord` and `Remove-DataverseRecord`. This provides a simple way to parallelize single operations without additional complexity.

Example with `Set-DataverseRecord`:

```powershell
# Create records in parallel using 4 workers with batches of 100
$records = 1..10000 | ForEach-Object { @{ firstname = "User$_"; lastname = "Parallel" } }
$records | Set-DataverseRecord -Connection $c -TableName contact -CreateOnly -MaxDegreeOfParallelism 4 -BatchSize 100 -Verbose
```

Example with `Remove-DataverseRecord`:

```powershell
# Delete records in parallel using 4 workers
$records = Get-DataverseRecord -Connection $c -TableName contact -Filter @{ status = 'inactive' }
$records | Remove-DataverseRecord -Connection $c -MaxDegreeOfParallelism 4 -Verbose
```

**For multi-step workflows or complex operations:** Use [`Invoke-DataverseParallel`](../../Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseParallel.md) when you need to perform multiple operations on each record or execute custom PowerShell logic in parallel. This cmdlet handles connection cloning, chunking, and parallel execution for you. It works on both PowerShell 5.1 and PowerShell 7+.

Example with `Invoke-DataverseParallel`:

```powershell
$connection = Get-DataverseConnection -url 'https://myorg.crm.dynamics.com' -ClientId $env:CLIENT_ID -ClientSecret $env:CLIENT_SECRET -TenantId $env:TENANT_ID
# Get records and update them in parallel
Get-DataverseRecord -Connection $connection -TableName contact -Top 1000 |
  Invoke-DataverseParallel -Connection $connection -ChunkSize 50 -MaxDegreeOfParallelism 8 -ScriptBlock {
    $_ |
       ForEach-Object{ $_.emailaddress1 = "updated-$($_.contactid)@example.com"; $_ } |
       Set-DataverseRecord -TableName contact -UpdateAllColumns
  }
```

Please read the full cmdlet documentation for more recommendations.


