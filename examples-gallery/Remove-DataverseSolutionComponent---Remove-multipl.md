---
title: "Remove-DataverseSolutionComponent - Remove multiple components from pipeline"
tags: ['Solutions']
source: "Remove-DataverseSolutionComponent.md"
---
Removes all view components (type 26) from a solution.

```powershell
$components = Get-DataverseSolutionComponent -SolutionName "MySolution"
$components | Where-Object { $_.ComponentType -eq 26 } | ForEach-Object {
    Remove-DataverseSolutionComponent -SolutionName "MySolution" `
       -ComponentId $_.ObjectId -ComponentType $_.ComponentType -Confirm:$false
}

```

