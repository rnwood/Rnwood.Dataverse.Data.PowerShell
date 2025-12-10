---
title: "Import-DataverseSolution - Skip validation for pre-configured environments"
tags: ['Solutions']
source: "Import-DataverseSolution.md"
---
Imports the solution and skips validation checks, useful when connection references and environment variables are already configured in the target environment.

```powershell
Import-DataverseSolution -InFile "C:\Solutions\MySolution.zip" -SkipConnectionReferenceValidation -SkipEnvironmentVariableValidation

```
