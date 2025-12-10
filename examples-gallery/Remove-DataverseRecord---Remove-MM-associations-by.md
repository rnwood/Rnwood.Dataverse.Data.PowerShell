---
title: "Remove-DataverseRecord - Remove M:M associations by deleting from intersect table"
tags: ['Data']
source: "Remove-DataverseRecord.md"
---
Removes a many-to-many association by deleting the corresponding record from the intersect table. This disassociates the two related entities without affecting the entities themselves.

```powershell
# Find the intersect record to delete
$intersectRecord = Get-DataverseRecord -TableName "account_accounts" -FilterValues @{
    accountid = $account1Id
    accountid2 = $account2Id
}

# Delete the association
$intersectRecord | Remove-DataverseRecord 

```

