---
title: "Get-DataverseFileData - Pipe from Get-DataverseRecord"
tags: ['Data']
source: "Get-DataverseFileData.md"
---
Pipes a record from Get-DataverseRecord and downloads its file.

```powershell
Get-DataverseRecord -TableName "account" -Id $accountId | Get-DataverseFileData -ColumnName "documentfile" -FolderPath "C:\Downloads"

```

