---
title: "Remove-DataverseRecord - Delete multiple matching records with AllowMultipleMatches"
tags: ['Data']
source: "Remove-DataverseRecord.md"
---
Deletes all contact records with the lastname "TestUser". The -AllowMultipleMatches switch allows deletion of multiple records that match the criteria.

```powershell
@{ lastname = "TestUser" } | Remove-DataverseRecord -TableName contact -MatchOn lastname -AllowMultipleMatches

```

