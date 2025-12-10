---
title: "Compare-DataverseSolutionComponents - Filter results to show only added components"
tags: ['Solutions']
source: "Compare-DataverseSolutionComponents.md"
---
This example shows only the components that have been added to the solution file but don't exist in the environment.

```powershell
$results = Compare-DataverseSolutionComponents -SolutionFile "C:\Solutions\MySolution.zip"
$results | Where-Object { $_.Status -eq "Added" } | Format-Table

```
