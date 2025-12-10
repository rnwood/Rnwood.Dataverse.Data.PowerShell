---
title: "Set-DataverseConnectionReference - Set connection references with stored connection IDs"
tags: ['Solutions']
source: "Set-DataverseConnectionReference.md"
---
Retrieves connection IDs by name and sets them for the connection references.

```powershell
$connectionIds = @{
    'new_dataverse' = (Get-DataverseRecord -TableName connection -Filter "name eq 'Production Dataverse'").connectionid
    'new_sharepoint' = (Get-DataverseRecord -TableName connection -Filter "name eq 'Production SharePoint'").connectionid
}
Set-DataverseConnectionReference -ConnectionReferences $connectionIds

```
