---
title: "Get-DataverseRecordsFolder Example"
tags: ['Data']
source: "Get-DataverseRecordsFolder.md"
---
Reads files from `data/contacts` and uses them to create/update records in Dataverse using the existing connection `$c`.
See documentation for `Set-DataverseRecord` as there are option to control how/if existing records will be matched and updated.

```powershell
Get-DataverseRecordsFolder -InputPath data/contacts | Set-DataverseRecord 

```

