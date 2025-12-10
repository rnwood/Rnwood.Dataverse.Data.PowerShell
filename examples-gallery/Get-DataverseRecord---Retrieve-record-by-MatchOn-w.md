---
title: "Get-DataverseRecord - Retrieve record by MatchOn with single column"
tags: ['Data']
source: "Get-DataverseRecord.md"
---
Retrieves a contact record by matching on the email address. If multiple contacts have the same email, an error is raised unless -AllowMultipleMatches is used.

```powershell
@{ emailaddress1 = "user@example.com" } | 
    Get-DataverseRecord -TableName contact -MatchOn emailaddress1

```

