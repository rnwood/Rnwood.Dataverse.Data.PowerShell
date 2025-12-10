---
title: "Get-DataverseSolutionFile - Check if a solution is managed"
tags: ['Solutions']
source: "Get-DataverseSolutionFile.md"
---
Parses the solution and checks the managed status.

```powershell
$info = Get-DataverseSolutionFile -Path "C:\Solutions\MySolution.zip"
if ($info.IsManaged) {
# >>     Write-Host "Solution is managed"
# >> } else {
# >>     Write-Host "Solution is unmanaged"
# >> }

```
