# Parallelizing Work for Best Performance

<!-- TOC -->
<!-- /TOC -->

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



