# Managing data in source control

- [`Get-DataverseRecordsFolder`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecordsFolder.md) and [`Set-DataverseRecordsFolder`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecordsFolder.md) are provided to read and write a folder of JSON records which is very suitable for source control — see `Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecordsFolder.md` and `Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecordsFolder.md`.

Example: apply a folder of data files during deployment (dry-run first). This reads files from the folder and pipes them into [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) to apply them to Dataverse. Using `account` as an example table name:

```powershell
# Complete example: export (including deletions) and apply to a target environment

# 1) Export from a source environment into ./data/my_table and write a `deletions/` subfolder
# Create connections (example):
$sourceConn = Get-DataverseConnection -url 'https://source.crm11.dynamics.com' -interactive
#
# The `-withdeletions` switch causes `Set-DataverseRecordsFolder` to record any items
# that were present previously but are not present in the new export into
# `./data/my_table/deletions/` so they can be removed from target environments.
Get-DataverseRecord -Connection $sourceConn -TableName my_table -Columns name,accountnumber,telephone1 |
  Set-DataverseRecordsFolder -OutputPath ./data/my_table -withdeletions

Now commit your files to source control etc.

Then bring them back when you are ready to deploy.

# 2) Apply the exported files to the target environment
# Create or obtain a target connection just before applying to the target
$targetConn = Get-DataverseConnection -url 'https://target.crm11.dynamics.com' -interactive

Get-DataverseRecordsFolder -InputPath ./data/my_table | Set-DataverseRecord -Connection $targetConn -BatchSize 100 -Verbose
Get-DataverseRecordsFolder -InputPath ./data/my_table -deletions | Remove-DataverseRecord -Connection $targetConn -BatchSize 100 -Verbose
```

### Copying data between environments

A simple, safe pattern to copy a set of records from a source environment to a target environment. This example uses the folder export helpers so you can review files before applying them to the target.

# 1) Export from the source environment
$src = Get-DataverseConnection -url 'https://source.crm11.dynamics.com' -interactive
Get-DataverseRecord -Connection $src -TableName account -Columns name,accountnumber,parentcustomerid |
  Set-DataverseRecordsFolder -OutputPath ./exports/account -WithDeletions

# 2) Import to the target environment (preview first with -WhatIf)
$dst = Get-DataverseConnection -url 'https://target.crm11.dynamics.com' -interactive
Get-DataverseRecordsFolder -InputPath ./exports/account | Set-DataverseRecord -Connection $dst -BatchSize 100 -Verbose -WhatIf

# 3) Apply deletions recorded during export (preview with -WhatIf first)
Get-DataverseRecordsFolder -InputPath ./exports/account -deletions | Remove-DataverseRecord -Connection $dst -BatchSize 100 -Verbose -WhatIf

Notes:
- Run the import with `-WhatIf` first to preview changes.
- Prefer preserving GUIDs (include `Id` on exported records) or use `-MatchOn`/`-Upsert` to avoid creating duplicates when re-importing.
- Test the process in a sandbox before running against production.




## See Also

- [Data Export](data-export.md) - Export data to various formats
- [Data Import](data-import.md) - Import data from files
- [CI/CD Pipelines](ci-cd-pipelines.md) - Automate deployments
