---
title: "Set-DataverseSolutionComponent - Add multiple components from pipeline"
tags: ['Solutions']
source: "Set-DataverseSolutionComponent.md"
---
Adds multiple components to a solution using pipeline input.

```powershell
$components = @(
    @{ ComponentId = "guid1"; ComponentType = 1 }
    @{ ComponentId = "guid2"; ComponentType = 26 }
    @{ ComponentId = "guid3"; ComponentType = 60 }
)
$components | ForEach-Object { 
    Set-DataverseSolutionComponent -SolutionName "MySolution" `
       -ComponentId $_.ComponentId -ComponentType $_.ComponentType -Behavior 0
}

```

