---
title: "Remove-DataverseRecord - Delete record using MatchOn with multiple columns"
tags: ['Data']
source: "Remove-DataverseRecord.md"
---
Deletes a contact record by matching on both firstname and lastname. This helps ensure you're deleting the correct record when names might not be unique individually.

```powershell
@{ firstname = "John"; lastname = "Doe" } | Remove-DataverseRecord -TableName contact -MatchOn @("firstname", "lastname")

```

