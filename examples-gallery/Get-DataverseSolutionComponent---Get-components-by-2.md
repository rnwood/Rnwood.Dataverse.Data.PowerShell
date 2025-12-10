---
title: "Get-DataverseSolutionComponent - Get components by solution name"
tags: ['Solutions']
source: "Get-DataverseSolutionComponent.md"
---
This example retrieves all root components from the solution named "MySolution".

```powershell
$conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
Get-DataverseSolutionComponent onn -SolutionName "MySolution"

```

