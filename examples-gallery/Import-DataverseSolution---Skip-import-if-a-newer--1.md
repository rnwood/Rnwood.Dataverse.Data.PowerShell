---
title: "Import-DataverseSolution - Skip import if a newer version is already installed"
tags: ['Solutions']
source: "Import-DataverseSolution.md"
---
Skips the import if the solution version in the file is lower than the version already installed in the target environment. Prevents accidental downgrades.

```powershell
Import-DataverseSolution -InFile "C:\Solutions\MySolution_1.0.0.0.zip" -SkipIfLowerVersion

```
