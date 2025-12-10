---
title: "Compare-DataverseSolutionComponents - Export comparison results to CSV"
tags: ['Solutions']
source: "Compare-DataverseSolutionComponents.md"
---
This example exports the comparison results to a CSV file for further analysis.

```powershell
$results = Compare-DataverseSolutionComponents -SolutionFile "C:\Solutions\MySolution.zip"
$results | Export-Csv -Path "C:\Reports\solution-comparison.csv" -NoTypeInformation

```
