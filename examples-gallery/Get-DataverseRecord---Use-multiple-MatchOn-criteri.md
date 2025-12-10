---
title: "Get-DataverseRecord - Use multiple MatchOn criteria with fallback"
tags: ['Data']
source: "Get-DataverseRecord.md"
---
Attempts to match first on emailaddress1, then falls back to matching on firstname+lastname if no email match is found. Uses the first matching set that returns records.

```powershell
@{ 
    emailaddress1 = "user@example.com"
    firstname = "John"
    lastname = "Doe"
} | Get-DataverseRecord -TableName contact -MatchOn @("emailaddress1"), @("firstname", "lastname")

```

