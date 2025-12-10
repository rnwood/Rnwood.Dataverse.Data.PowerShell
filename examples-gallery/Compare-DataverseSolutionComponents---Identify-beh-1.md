---
title: "Compare-DataverseSolutionComponents - Identify behavior changes between solution versions"
tags: ['Solutions']
source: "Compare-DataverseSolutionComponents.md"
---
This example compares two solution versions and shows only components where the behavior has changed (BehaviorIncluded or BehaviorExcluded).

```powershell
$results = Compare-DataverseSolutionComponents -SolutionFile "C:\v1\MySolution.zip" -TargetSolutionFile "C:\v2\MySolution.zip"
$results | Where-Object { $_.Status -like "Behavior*" } | Format-Table

```
