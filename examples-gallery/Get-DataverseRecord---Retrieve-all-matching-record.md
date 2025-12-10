---
title: "Get-DataverseRecord - Retrieve all matching records with AllowMultipleMatches"
tags: ['Data']
source: "Get-DataverseRecord.md"
---
Retrieves ALL contact records with the lastname "Smith". The -AllowMultipleMatches switch allows retrieving multiple records that match the criteria. Without this switch, an error would be raised if multiple matches are found.

```powershell
@{ lastname = "Smith" } | 
    Get-DataverseRecord -TableName contact -MatchOn lastname -AllowMultipleMatches

```

