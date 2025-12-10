---
title: "Set-DataverseFileData - Use WhatIf to preview changes"
tags: ['Data']
source: "Set-DataverseFileData.md"
---
Shows what would happen if the cmdlet runs without actually uploading the file.

```powershell
Set-DataverseFileData -TableName "account" -Id $accountId -ColumnName "documentfile" -FilePath "C:\Documents\contract.pdf" -WhatIf

```

