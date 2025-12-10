---
title: "Set-DataverseRecord - Batch create multiple records"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Creates multiple contact records in a single batch. The `-CreateOnly` switch improves performance by skipping the existence check since we know these are new records. By default, all 3 records will be sent in a single ExecuteMultipleRequest.

```powershell
$contacts = @(
    @{ firstname = "John"; lastname = "Doe"; emailaddress1 = "john@example.com" }
    @{ firstname = "Jane"; lastname = "Smith"; emailaddress1 = "jane@example.com" }
    @{ firstname = "Bob"; lastname = "Johnson"; emailaddress1 = "bob@example.com" }
)

$contacts | Set-DataverseRecord -TableName contact -CreateOnly

```

