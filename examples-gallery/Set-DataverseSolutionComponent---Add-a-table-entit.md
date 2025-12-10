---
title: "Set-DataverseSolutionComponent - Add a table (entity) to a solution"
tags: ['Solutions']
source: "Set-DataverseSolutionComponent.md"
---
Adds an entity (ComponentType 1) to "MySolution" with behavior "Include Subcomponents" (0).

```powershell
$connection = Get-DataverseConnection -Url "https://contoso.crm.dynamics.com" -Interactive
Set-DataverseSolutionComponent -SolutionName "MySolution" `
   -ComponentId "dfe12c85-55b3-4c77-9c04-7d5d06d2e9e2" -ComponentType 1 -Behavior 0

```

