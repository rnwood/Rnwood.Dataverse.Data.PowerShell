---
title: "Export-DataverseSolution - Export solution with custom timeout"
tags: ['Solutions']
source: "Export-DataverseSolution.md"
---
Exports a large solution with a 20-minute timeout and checks status every 10 seconds.

```powershell
Export-DataverseSolution -SolutionName "LargeSolution" -OutFile "C:\Exports\LargeSolution.zip" -TimeoutSeconds 1200 -PollingIntervalSeconds 10

```
