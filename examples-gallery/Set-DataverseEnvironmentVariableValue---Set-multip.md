---
title: "Set-DataverseEnvironmentVariableValue - Set multiple environment variable values"
tags: ['Solutions']
source: "Set-DataverseEnvironmentVariableValue.md"
---
Sets values for multiple environment variables at once using a hashtable.

```powershell
Set-DataverseEnvironmentVariableValue -EnvironmentVariableValues @{
    'new_apiurl' = 'https://api.production.example.com'
    'new_apikey' = 'prod-key-12345'
    'new_timeout' = '30'
}

```
