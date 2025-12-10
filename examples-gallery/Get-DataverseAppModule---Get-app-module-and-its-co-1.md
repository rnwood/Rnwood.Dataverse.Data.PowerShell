---
title: "Get-DataverseAppModule - Get app module and its components"
tags: ['Metadata']
source: "Get-DataverseAppModule.md"
---
Gets an app module and then retrieves all its components. (Parameter name corrected: use -AppModuleId not -AppModuleIdValue.)

```powershell
$app = Get-DataverseAppModule -UniqueName "myapp"
$components = Get-DataverseAppModuleComponent -AppModuleId $app.Id

```
