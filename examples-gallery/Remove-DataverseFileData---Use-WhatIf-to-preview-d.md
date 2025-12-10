---
title: "Remove-DataverseFileData - Use WhatIf to preview deletions"
tags: ['Data']
source: "Remove-DataverseFileData.md"
---
Shows what would happen if the cmdlet runs without actually deleting the file.

```powershell
Remove-DataverseFileData -TableName "account" -Id $accountId -ColumnName "documentfile" -WhatIf

```

