---
title: "Set-DataverseFileData - Pipe from Get-DataverseRecord"
tags: ['Data']
source: "Set-DataverseFileData.md"
---
Pipes a record and uploads a file to it.

```powershell
Get-DataverseRecord -TableName "account" -Id $accountId | Set-DataverseFileData -ColumnName "documentfile" -FilePath "C:\Documents\contract.pdf"

```

