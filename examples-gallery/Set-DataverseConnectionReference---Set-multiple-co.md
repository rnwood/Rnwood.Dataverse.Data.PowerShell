---
title: "Set-DataverseConnectionReference - Set multiple connection references"
tags: ['Solutions']
source: "Set-DataverseConnectionReference.md"
---
Sets multiple connection references at once using a hashtable.

```powershell
Set-DataverseConnectionReference -ConnectionReferences @{
    'new_sharepoint' = '12345678-1234-1234-1234-123456789012'
    'new_sql' = '87654321-4321-4321-4321-210987654321'
}

```
