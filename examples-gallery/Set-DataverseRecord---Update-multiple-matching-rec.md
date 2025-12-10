---
title: "Set-DataverseRecord - Update multiple matching records with AllowMultipleMatches"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Updates ALL contacts with lastname "TestUser" by setting their email address. The -AllowMultipleMatches switch allows updating all matching records when multiple records match the MatchOn criteria. Without this switch, an error would be raised if multiple matches are found.

```powershell
# Update all contacts with a specific last name
@{ 
    lastname = "TestUser"
    emailaddress1 = "updated@example.com" 
} | Set-DataverseRecord -TableName contact -MatchOn lastname -AllowMultipleMatches

```

