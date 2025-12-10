---
title: "Import-DataverseSolution - Import with custom timeout"
tags: ['Solutions']
source: "Import-DataverseSolution.md"
---
Imports a large solution with a 60-minute timeout and checks status every 10 seconds.

```powershell
Import-DataverseSolution -InFile "C:\Solutions\LargeSolution.zip" -TimeoutSeconds 3600 -PollingIntervalSeconds 10

```
