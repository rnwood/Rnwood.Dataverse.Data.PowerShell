---
title: "Import-DataverseSolution - Explicit stage and upgrade (when conditions are met)"
tags: ['Solutions']
source: "Import-DataverseSolution.md"
---
Explicitly requests stage and upgrade mode. The cmdlet will check if the solution exists and use StageAndUpgradeAsyncRequest if it does, otherwise falls back to regular import.

```powershell
Import-DataverseSolution -InFile "C:\Solutions\MyManagedSolution.zip" -Mode StageAndUpgrade

```
