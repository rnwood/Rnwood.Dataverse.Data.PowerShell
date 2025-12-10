---
title: "Set-DataverseRecord - Use PassThru to create and link records"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Demonstrates using `-PassThru` to chain record creation operations. First creates an account and captures its ID using `-PassThru`, then uses that ID to create a related contact record. This pattern is useful when you need to establish relationships between newly created records.

```powershell
# Create parent account and get its ID
$account = @{ name = "Contoso Ltd" } | 
    Set-DataverseRecord -TableName account -CreateOnly -PassThru

# Create child contact linked to the account
$contact = @{ 
    firstname = "John"
    lastname = "Doe"
    parentcustomerid = $account.Id
} | Set-DataverseRecord -TableName contact -CreateOnly -PassThru

Write-Host "Created contact $($contact.Id) linked to account $($account.Id)"

```

