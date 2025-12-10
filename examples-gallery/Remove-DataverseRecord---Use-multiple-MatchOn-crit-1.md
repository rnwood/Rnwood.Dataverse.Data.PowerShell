---
title: "Remove-DataverseRecord - Use multiple MatchOn criteria with fallback"
tags: ['Data']
source: "Remove-DataverseRecord.md"
---
Attempts to match first on emailaddress1, then falls back to matching on firstname+lastname if no email match is found. Uses the first matching set that returns records.

```powershell
$record = @{ 
    emailaddress1 = "user@example.com"
    firstname = "John"
    lastname = "Doe"
}
$record | Remove-DataverseRecord -TableName contact -MatchOn @("emailaddress1"), @("firstname", "lastname")

```

