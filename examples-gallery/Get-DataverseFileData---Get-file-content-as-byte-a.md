---
title: "Get-DataverseFileData - Get file content as byte array"
tags: ['Data']
source: "Get-DataverseFileData.md"
---
Retrieves the file content as a byte array for in-memory processing.

```powershell
$bytes = Get-DataverseFileData -TableName "account" -Id $accountId -ColumnName "documentfile" -AsBytes

```

