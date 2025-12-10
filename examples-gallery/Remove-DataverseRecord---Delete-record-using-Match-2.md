---
title: "Remove-DataverseRecord - Delete record using MatchOn with single column"
tags: ['Data']
source: "Remove-DataverseRecord.md"
---
Deletes a contact record by matching on the email address. If multiple contacts have the same email, an error is raised unless -AllowMultipleMatches is used.

```powershell
@{ emailaddress1 = "user@example.com" } | Remove-DataverseRecord -TableName contact -MatchOn emailaddress1

```

