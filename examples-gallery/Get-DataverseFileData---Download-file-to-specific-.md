---
title: "Get-DataverseFileData - Download file to specific path"
tags: ['Data']
source: "Get-DataverseFileData.md"
---
Downloads the file from the specified column to the given file path.

```powershell
Get-DataverseFileData -TableName "account" -Id $accountId -ColumnName "documentfile" -FilePath "C:\Downloads\contract.pdf"

```

