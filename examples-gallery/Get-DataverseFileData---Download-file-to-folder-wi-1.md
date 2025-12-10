---
title: "Get-DataverseFileData - Download file to folder with original filename"
tags: ['Data']
source: "Get-DataverseFileData.md"
---
Downloads the file using its original filename into the specified folder.

```powershell
Get-DataverseFileData -TableName "account" -Id $accountId -ColumnName "documentfile" -FolderPath "C:\Downloads"

```

