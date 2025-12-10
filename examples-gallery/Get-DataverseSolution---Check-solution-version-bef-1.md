---
title: "Get-DataverseSolution - Check solution version before upgrade"
tags: ['Solutions']
source: "Get-DataverseSolution.md"
---
Retrieves a solution and checks if it needs to be upgraded.

```powershell
$solution = Get-DataverseSolution -UniqueName "MySolution"
if ($solution.Version -lt [Version]"2.0.0.0") {
# >>     Write-Host "Solution needs upgrade from $($solution.Version) to 2.0.0.0"
# >> }

```
