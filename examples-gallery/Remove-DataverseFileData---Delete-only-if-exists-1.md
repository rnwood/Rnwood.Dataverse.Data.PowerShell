---
title: "Remove-DataverseFileData - Delete only if exists"
tags: ['Data']
source: "Remove-DataverseFileData.md"
---
Deletes the file if it exists, but doesn't raise an error if it doesn't exist.

```powershell
Remove-DataverseFileData -TableName "account" -Id $accountId -ColumnName "documentfile" -IfExists

```

