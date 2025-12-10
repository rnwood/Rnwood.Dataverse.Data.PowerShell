---
title: "Get-DataverseSolutionComponent - Count components by type"
tags: ['Solutions']
source: "Get-DataverseSolutionComponent.md"
---
This example retrieves all components and groups them by component type to show a count of each type.

```powershell
$conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
$components = Get-DataverseSolutionComponent onn -SolutionName "MySolution"
$components | Group-Object ComponentTypeName | Select-Object Name, Count | Sort-Object Count -Descending

```

