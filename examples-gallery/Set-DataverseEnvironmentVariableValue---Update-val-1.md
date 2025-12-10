---
title: "Set-DataverseEnvironmentVariableValue - Update value from pipeline"
tags: ['Solutions']
source: "Set-DataverseEnvironmentVariableValue.md"
---
Updates the value for an existing environment variable.

```powershell
Get-DataverseEnvironmentVariableValue -SchemaName "new_apiurl" | 
    Set-DataverseEnvironmentVariableValue -Value "https://api.staging.example.com"

```
