---
title: "Get-DataverseSolutionComponent - Get components including subcomponents"
tags: ['Solutions']
source: "Get-DataverseSolutionComponent.md"
---
This example retrieves all root components and their subcomponents from the solution named "MySolution".

```powershell
$conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
Get-DataverseSolutionComponent onn -SolutionName "MySolution" -IncludeImpliedSubcomponents

```

