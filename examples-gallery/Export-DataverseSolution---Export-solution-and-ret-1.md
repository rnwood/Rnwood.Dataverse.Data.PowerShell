---
title: "Export-DataverseSolution - Export solution and return bytes to pipeline"
tags: ['Solutions']
source: "Export-DataverseSolution.md"
---
Exports "MySolution" and captures the raw bytes in a variable for further processing.

```powershell
$solutionBytes = Export-DataverseSolution -SolutionName "MySolution" -PassThru
[System.IO.File]::WriteAllBytes("C:\Exports\MySolution.zip", $solutionBytes)

```
