---
title: "Remove-DataverseFileData - Pipe from Get-DataverseRecord to delete multiple files"
tags: ['Data']
source: "Remove-DataverseFileData.md"
---
Pipes multiple records and deletes files from all of them.

```powershell
Get-DataverseRecord -TableName "account" -FilterValues @{deletefiles=$true} | Remove-DataverseFileData -ColumnName "documentfile" -IfExists

```

