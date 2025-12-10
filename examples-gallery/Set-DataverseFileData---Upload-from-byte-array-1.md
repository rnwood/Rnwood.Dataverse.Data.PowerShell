---
title: "Set-DataverseFileData - Upload from byte array"
tags: ['Data']
source: "Set-DataverseFileData.md"
---
Uploads file content from a byte array with a specified filename.

```powershell
$bytes = [System.IO.File]::ReadAllBytes("C:\Documents\contract.pdf")
Set-DataverseFileData -TableName "account" -Id $accountId -ColumnName "documentfile" -FileContent $bytes -FileName "contract.pdf"

```

