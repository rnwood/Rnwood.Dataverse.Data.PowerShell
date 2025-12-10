---
title: "Remove-DataverseAppModuleComponent - Remove a component by ID"
tags: ['Metadata']
source: "Remove-DataverseAppModuleComponent.md"
---
Finds and removes a specific component from an app module using the component ID.

```powershell
$component = Get-DataverseAppModuleComponent -AppModuleUniqueName "myapp" | Where-Object { $_.ObjectId -eq $entityId }
Remove-DataverseAppModuleComponent -Id $component.Id -Confirm:$false

```

