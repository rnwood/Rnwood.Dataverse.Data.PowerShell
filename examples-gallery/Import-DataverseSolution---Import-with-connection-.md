---
title: "Import-DataverseSolution - Import with connection references and environment variables"
tags: ['Solutions']
source: "Import-DataverseSolution.md"
---
Imports the solution and sets connection references for two connections and environment variables for two settings.

```powershell
Import-DataverseSolution -InFile "C:\Solutions\MySolution.zip" `
   -ConnectionReferences @{
        'new_sharepoint' = '12345678-1234-1234-1234-123456789012'
        'new_sql' = '87654321-4321-4321-4321-210987654321'
    } `
   -EnvironmentVariables @{
        'new_apiurl' = 'https://api.production.example.com'
        'new_apikey' = 'prod-key-12345'
    }

```
