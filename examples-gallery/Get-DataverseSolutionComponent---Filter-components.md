---
title: "Get-DataverseSolutionComponent - Filter components by type"
tags: ['Solutions']
source: "Get-DataverseSolutionComponent.md"
---
This example retrieves all components from the solution and filters to show only entities (ComponentType 1).

```powershell
$conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
$components = Get-DataverseSolutionComponent onn -SolutionName "MySolution"
$components | Where-Object { $_.ComponentType -eq 1 } | Format-Table

```

