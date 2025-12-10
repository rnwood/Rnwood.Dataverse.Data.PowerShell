---
title: "Set-DataverseRecord - Upsert with alternate key"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Uses upsert operation with alternate key on emailaddress1. If a contact with the email exists, it will be updated; otherwise a new one is created. Requires that `emailaddress1` is defined as an alternate key on the contact table.

```powershell
@(
    @{ emailaddress1 = "user1@example.com"; firstname = "Alice"; lastname = "Anderson" }
    @{ emailaddress1 = "user2@example.com"; firstname = "Bob"; lastname = "Brown" }
) | Set-DataverseRecord -TableName contact -Upsert -MatchOn emailaddress1

```

