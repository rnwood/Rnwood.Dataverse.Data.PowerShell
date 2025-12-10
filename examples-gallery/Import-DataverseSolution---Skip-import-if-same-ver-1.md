---
title: "Import-DataverseSolution - Skip import if same version is already installed"
tags: ['Solutions']
source: "Import-DataverseSolution.md"
---
Skips the import if the solution version in the file (e.g., 1.0.0.0) is the same as the version already installed in the target environment. Useful for deployment scripts that should be idempotent.

```powershell
Import-DataverseSolution -InFile "C:\Solutions\MySolution_1.0.0.0.zip" -SkipIfSameVersion

```
