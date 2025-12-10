---
title: "Set-DataverseRecord - Get IDs of created records using PassThru"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Creates multiple contact records and returns the input objects with their `Id` property set. The `-PassThru` parameter causes the cmdlet to write each input object to the pipeline after the record is created, with the `Id` property populated with the GUID of the newly created record. This allows you to capture and use the IDs in subsequent operations.

```powershell
$contacts = @(
    @{ firstname = "Alice"; lastname = "Anderson" }
    @{ firstname = "Bob"; lastname = "Brown" }
    @{ firstname = "Charlie"; lastname = "Clark" }
)

$createdRecords = $contacts | Set-DataverseRecord -TableName contact -CreateOnly -PassThru

# Access the IDs of created records
foreach ($record in $createdRecords) {
    Write-Host "Created contact $($record.firstname) $($record.lastname) with ID: $($record.Id)"
}

```

