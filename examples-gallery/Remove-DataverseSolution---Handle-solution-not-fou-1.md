---
title: "Remove-DataverseSolution - Handle solution not found error"
tags: ['Solutions']
source: "Remove-DataverseSolution.md"
---
Attempts to remove a solution that doesn't exist and handles the error.

```powershell
try {
# >>     Remove-DataverseSolution -UniqueName "NonExistentSolution" -ErrorAction Stop
# >> } catch {
# >>     Write-Host "Error: $_"
# >> }
# Error: Solution 'NonExistentSolution' not found.

```
