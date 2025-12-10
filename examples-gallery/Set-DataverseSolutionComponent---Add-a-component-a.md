---
title: "Set-DataverseSolutionComponent - Add a component and include required dependencies"
tags: ['Solutions']
source: "Set-DataverseSolutionComponent.md"
---
Adds a view (ComponentType 26) and automatically includes any required components (like the parent entity).

```powershell
Set-DataverseSolutionComponent -SolutionName "MySolution" `
   -ComponentId "8a9b7c6d-5e4f-3a2b-1c0d-9e8f7a6b5c4d" -ComponentType 26 `
   -AddRequiredComponents -Behavior 0

```

