---
title: "Remove-DataverseEnvironmentVariableValue - Remove from pipeline"
tags: ['Solutions']
source: "Remove-DataverseEnvironmentVariableValue.md"
---
Removes values for all environment variables with schema names starting with "new_test".

```powershell
Get-DataverseEnvironmentVariableValue -SchemaName "new_test*" | 
    Remove-DataverseEnvironmentVariableValue

```
