---
title: "Get-DataverseSolutionComponent - Get components by solution ID"
tags: ['Solutions']
source: "Get-DataverseSolutionComponent.md"
---
This example retrieves all root components from the solution with the specified ID.

```powershell
$conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
Get-DataverseSolutionComponent onn -SolutionId "12345678-1234-1234-1234-123456789012"

```

