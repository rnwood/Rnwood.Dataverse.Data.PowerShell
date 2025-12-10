---
title: "Set-DataverseRecord - Batch associate multiple M:M relationships"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Creates multiple many-to-many associations in a batch operation. This example associates multiple contacts with a marketing list using the `listcontact` intersect table. The property names (`contactid`, `listid`) must match the exact column names in the intersect table. The cmdlet automatically batches these operations for improved performance.

```powershell
# Create multiple contacts and marketing lists
$contacts = @(
    @{ firstname = "Alice"; lastname = "Smith" }
    @{ firstname = "Bob"; lastname = "Jones" }
    @{ firstname = "Carol"; lastname = "Williams" }
) | Set-DataverseRecord -TableName contact -CreateOnly -PassThru

$marketingList = @{ listname = "Q1 Newsletter Subscribers"; type = $false } | 
    Set-DataverseRecord -TableName list -CreateOnly -PassThru

# Associate all contacts with the marketing list using the intersect table
# Property names must match the exact column names in the intersect table
$associations = $contacts | ForEach-Object {
    @{
        contactid = $_.Id
        listid = $marketingList.Id
    }
}

$associations | Set-DataverseRecord -TableName "listcontact" -CreateOnly

```

