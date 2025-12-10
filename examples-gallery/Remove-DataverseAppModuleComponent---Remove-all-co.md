---
title: "Remove-DataverseAppModuleComponent - Remove all components of a type"
tags: ['Metadata']
source: "Remove-DataverseAppModuleComponent.md"
---
Removes all chart components from an app module using pipeline input with the ById parameter set.

```powershell
Get-DataverseAppModuleComponent -AppModuleUniqueName "myapp" -ComponentType Chart |
    Remove-DataverseAppModuleComponent -Confirm:$false

```

