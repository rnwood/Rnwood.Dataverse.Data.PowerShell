---
title: "Import-DataverseSolution - Combine version checks"
tags: ['Solutions']
source: "Import-DataverseSolution.md"
---
Skips the import if the solution version in the file is the same as or lower than the version installed. Only imports if the file contains a newer version.

```powershell
Import-DataverseSolution -InFile "C:\Solutions\MySolution.zip" -SkipIfSameVersion -SkipIfLowerVersion

```
