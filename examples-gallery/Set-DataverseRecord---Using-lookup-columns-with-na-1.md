---
title: "Set-DataverseRecord - Using lookup columns with names"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Creates a contact with a lookup to an account by name. The module will automatically resolve "Contoso Ltd" to the account's GUID if the name is unique.

```powershell
@{
    firstname = "John"
    lastname = "Doe"
    parentcustomerid = "Contoso Ltd"  # Lookup by account name
} | Set-DataverseRecord -TableName contact

```

