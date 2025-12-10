---
title: "Set-DataverseRecord - Associate records in many-to-many relationships"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Creates a many-to-many association between two account records. Many-to-many relationships in Dataverse are implemented using intersect tables that contain references to both related entities. The property names must match the exact column names in the intersect table (typically the primary key column names of the related entities). Use metadata queries or the Dataverse UI to find the correct table name and column names for your M:M relationship.

```powershell
# Create two accounts to associate
$account1 = @{ name = "Contoso Ltd" } | Set-DataverseRecord -TableName account -CreateOnly -PassThru
$account2 = @{ name = "Fabrikam Inc" } | Set-DataverseRecord -TableName account -CreateOnly -PassThru

# Associate the accounts using the intersect table
or similar
# For account-to-account relationships, it might be "account_accounts" or similar
# Check your Dataverse metadata for the exact intersect table name and column names
@{
    # Entity 1 in the relationship - use the exact column name from the intersect table
    accountid = $account1.Id
    
    # Entity 2 in the relationship - use the exact column name from the intersect table
    accountid2 = $account2.Id
} | Set-DataverseRecord -TableName "account_accounts" -CreateOnly

```

