# Mass updating data

Mass updates are a common operational task — fixing a field across many records, applying a business-rule-driven change, or backfilling values. Choose the approach that balances safety, performance, and the need to execute platform business logic (plugins/workflows):

- For medium-to-large workloads consider using SQL ([`Invoke-DataverseSql`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md)) for bulk updates when appropriate. SQL can be high-performance, but the SQL engine may retrieve and process data locally and SQL statements bypass the module's conversion logic — be careful with types, lookups and side effects.
- Use [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) when you need the module's typed conversions, lookup resolution, and to honour platform business logic by default.
- Use batching (`-BatchSize`) and chunking (`Get-Chunks`) to control memory and throughput.
- Always test changes in a sandbox and perform dry-runs with `-WhatIf` before applying destructive updates.
- Consider `-BypassCustomPluginExecution` if you intentionally want to skip custom plugins/workflows (use with extreme caution).

Common pattern (safe, auditable): query the rows you need, transform locally, then push updates in batches. This lets you preview changes and capture what changed for auditing:

```powershell
# 1) Query candidates
$candidates = Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{ statuscode = 0 } -Columns contactid,description -Top 10000

# 2) Transform locally (preview)
$preview = $candidates | ForEach-Object {
  $_.description = "Updated on $(Get-Date -Format o)"
  $_
}

# Preview the first few changes
$preview | Select-Object -First 10 | Format-Table contactid,description

# 3) Apply in batches
$preview | Get-Chunks -ChunkSize 200 | ForEach-Object {
  $_ | Set-DataverseRecord -Connection $c -BatchSize 100 -Verbose -WhatIf
}
```

### Using SQL

Use [`Invoke-DataverseSql`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) when you need high performance for mass updates. The SQL engine may translate and perform some work client-side; SQL updates are fast but bypass the module's conversion logic — be careful with types and lookups.

Example: parameterised update (dry-run using `-WhatIf` / review first):

```powershell
Invoke-DataverseSql -Connection $c -Sql "UPDATE Contact SET description = @desc WHERE createdon < @cutoff" -Parameters @{ desc = 'Archival note'; cutoff = '2024-01-01' } -WhatIf
```

Example: update in two phases (select ids, then update via SQL or Set-DataverseRecord):

```powershell
# Phase 1: select ids (safe to review)
$ids = Invoke-DataverseSql -Connection $c -Sql "SELECT contactid FROM Contact WHERE ..." | Select-Object -ExpandProperty contactid

# Phase 2a: update via SQL in batches
$ids | ForEach-Object -Begin { $batch = @() } -Process {
  $batch += $_
  if ($batch.Count -ge 500) {
    $sql = "UPDATE Contact SET description = 'Batch update' WHERE contactid IN ('" + ($batch -join "','") + "')"
    Invoke-DataverseSql -Connection $c -Sql $sql -Verbose
    $batch = @()
  }
} -End {
  if ($batch.Count) {
    $sql = "UPDATE Contact SET description = 'Batch update' WHERE contactid IN ('" + ($batch -join "','") + "')"
    Invoke-DataverseSql -Connection $c -Sql $sql -Verbose
  }
}
```

Notes on SQL updates:
- Parameterise where possible to avoid SQL injection and improve readability.
- Use `-Timeout`, `-BatchSize` and `-MaxDegreeOfParallelism` on [`Invoke-DataverseSql`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) to control execution for large workloads.
- SQL updates may still trigger platform business logic; test to confirm behaviour or use `-BypassCustomPluginExecution` when supported and appropriate.

## Parallelization for Mass Updates

For updating large numbers of records, parallelization can dramatically reduce processing time by executing multiple update operations concurrently. The `Set-DataverseRecord` cmdlet includes built-in support for parallel processing.

*Example: Update 50,000 records in parallel:*
```powershell
# Query records that need updating
$records = Get-DataverseRecord -Connection $c -TableName contact `
  -FilterValues @{ statuscode = 1 } `
  -Columns contactid,description `
  -Top 50000

# Update in parallel using 6 workers with batches of 100
$records | ForEach-Object {
  $_.description = "Bulk updated on $(Get-Date -Format 'yyyy-MM-dd')"
  $_
} | Set-DataverseRecord -Connection $c `
    -BatchSize 100 `
    -MaxDegreeOfParallelism 6 `
    -Verbose
```

**Performance considerations:**
- **Parallelism level**: Start with `-MaxDegreeOfParallelism 4-8` and adjust based on your environment's throttling limits
- **Batch size**: Use `-BatchSize 50-100` for optimal balance between throughput and API limits
- **Error handling**: Combine with `-Retries 3` to automatically retry transient failures
- **Monitoring**: Use `-Verbose` to track progress and identify bottlenecks

*Example: Parallel updates with retry logic:*
```powershell
Get-DataverseRecord -Connection $c -TableName account -FilterValues @{ industry = 'Technology' } |
  ForEach-Object { $_.description = "Tech company updated"; $_ } |
  Set-DataverseRecord -Connection $c `
    -MaxDegreeOfParallelism 4 `
    -BatchSize 100 `
    -Retries 3 `
    -Verbose
```

For complex multi-step update workflows or when you need more control over the parallel execution, use `Invoke-DataverseParallel`. This cmdlet handles connection cloning and custom logic per record or batch.

See [Parallelization](../advanced/parallelization.md) for comprehensive guidance on parallel processing strategies, performance tuning, throttling management, and best practices for high-volume operations.

## See Also

- [Creating and Updating Records](../core-concepts/creating-updating.md) - Details on Set-DataverseRecord
- [Querying Records](../core-concepts/querying.md) - Filtering and querying data
- [Parallelization](../advanced/parallelization.md) - Parallel processing for best performance
- [Error Handling and Batch Operations](../core-concepts/error-handling.md) - Error handling strategies
