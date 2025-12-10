---
title: "Set-DataverseFileData - Upload with manual MIME type"
tags: ['Data']
source: "Set-DataverseFileData.md"
---
Uploads a file with a manually specified MIME type, overriding auto-detection.

```powershell
Set-DataverseFileData -TableName "account" -Id $accountId -ColumnName "documentfile" -FilePath "C:\Documents\data.bin" -MimeType "application/octet-stream"

```

