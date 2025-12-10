---
title: "Remove-DataverseSolutionComponent - Use IfExists to avoid errors"
tags: ['Solutions']
source: "Remove-DataverseSolutionComponent.md"
---
Removes the component if it exists in the solution, or does nothing if it doesn't exist (no error thrown).

```powershell
Remove-DataverseSolutionComponent -SolutionName "MySolution" `
   -ComponentId $componentId -ComponentType 1 -IfExists

```

