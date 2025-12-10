---
title: "Remove-DataverseEnvironmentVariableDefinition - Remove from pipeline"
tags: ['Solutions']
source: "Remove-DataverseEnvironmentVariableDefinition.md"
---
Removes all environment variable definitions with schema names starting with "new_test".

```powershell
Get-DataverseEnvironmentVariableDefinition -SchemaName "new_test*" | 
    Remove-DataverseEnvironmentVariableDefinition

```
