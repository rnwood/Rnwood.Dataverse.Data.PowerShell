# Deleting Records

<!-- TOC -->
<!-- /TOC -->

> [!WARNING]
> Deleting records is irreversible. Always preview deletions with `-WhatIf` and/or require confirmation with `-Confirm` when running destructive operations.

Use [`Remove-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md) to delete records by Id or via the pipeline. `-WhatIf` and `-Confirm` are supported to preview or require confirmation.

Basic examples:
```powershell
# Delete a single record by Id
Remove-DataverseRecord -Connection $c -TableName contact -Id '00000000-0000-0000-0000-000000000000'
# Delete records returned from a query (prompt for confirmation)
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{ lastname = 'TestUser' } |
  Remove-DataverseRecord -Connection $c -Confirm
# Preview deletes with WhatIf
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{ lastname = 'TestUser' } |
  Remove-DataverseRecord -Connection $c -WhatIf
# Batch delete with a specific batch size
Get-DataverseRecord -Connection $c -TableName account -Top 500 |
  Remove-DataverseRecord -Connection $c -BatchSize 50 -WhatIf
```

When deleting many records, batching is used to improve performance. Errors are returned per-record so you can correlate failures to the original inputs.

#### Deleting only if the record still exists

- [`-IfExists`](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md#-ifexists): Only attempts to delete the record if it exists, avoiding errors when the record may have already been deleted. As standard that's an error.

Examples:

```powershell
# Delete only if the record exists
Remove-DataverseRecord -Connection $c -TableName contact -Id '00000000-0000-0000-0000-000000000000' -IfExists
```
##### SQL alternative — Delete
You can perform deletes using [`Invoke-DataverseSql`](../../Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) (DELETE statements). For large deletes consider `-UseBulkDelete`. DML via SQL honours `ShouldProcess` so `-WhatIf`/`-Confirm` are supported. Example:
```powershell
Invoke-DataverseSql -Connection $c -Sql "DELETE FROM Contact WHERE statuscode = 2"  -WhatIf
```
