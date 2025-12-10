---
title: "Import-DataverseSolution - Import as holding solution (upgrade)"
tags: ['Solutions']
source: "Import-DataverseSolution.md"
---
Imports the solution as a holding solution for upgrade. If the solution doesn't already exist, it automatically falls back to a regular import.

```powershell
Import-DataverseSolution -InFile "C:\Solutions\MySolution_v2.zip" -Mode HoldingSolution

```
