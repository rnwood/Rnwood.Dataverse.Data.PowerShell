---
title: "Set-DataverseConnectionReference - Use with solution import workflow"
tags: ['Solutions']
source: "Set-DataverseConnectionReference.md"
---
Sets connection references before importing a solution, ensuring they are configured correctly.

```powershell
# Set connection references before importing solution
Set-DataverseConnectionReference -ConnectionReferences @{
    'new_sharepoint' = '12345678-1234-1234-1234-123456789012'
    'new_sql' = '87654321-4321-4321-4321-210987654321'
}
Import-DataverseSolution -InFile "solution.zip"

```
