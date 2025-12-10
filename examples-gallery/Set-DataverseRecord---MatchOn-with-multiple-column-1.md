---
title: "Set-DataverseRecord - MatchOn with multiple columns"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Looks for existing contacts matching BOTH firstname AND lastname. If found, updates them; otherwise creates new records.

```powershell
@(
    @{ firstname = "John"; lastname = "Doe"; telephone1 = "555-0001" }
    @{ firstname = "Jane"; lastname = "Smith"; telephone1 = "555-0002" }
) | Set-DataverseRecord -TableName contact -MatchOn ("firstname", "lastname")

```

