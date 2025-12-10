---
title: "Get-DataverseRecord - Retrieve record by MatchOn with multiple columns"
tags: ['Data']
source: "Get-DataverseRecord.md"
---
Retrieves a contact record by matching on both firstname and lastname together. This helps ensure you're retrieving the correct record when names might not be unique individually.

```powershell
@{ firstname = "John"; lastname = "Doe" } | 
    Get-DataverseRecord -TableName contact -MatchOn @("firstname", "lastname")

```

