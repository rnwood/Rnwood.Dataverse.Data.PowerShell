---
title: "Set-DataverseFileData - Upload file from path"
tags: ['Data']
source: "Set-DataverseFileData.md"
---
Uploads a file from the specified path to the file column. MIME type is automatically detected as "application/pdf".

```powershell
Set-DataverseFileData -TableName "account" -Id $accountId -ColumnName "documentfile" -FilePath "C:\Documents\contract.pdf"

```

