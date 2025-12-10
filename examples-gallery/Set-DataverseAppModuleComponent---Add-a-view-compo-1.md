---
title: "Set-DataverseAppModuleComponent - Add a view component"
tags: ['Metadata']
source: "Set-DataverseAppModuleComponent.md"
---
Adds a view with subcomponents included.

```powershell
$view = Get-DataverseView -TableName contact -Name "Active Contacts"
Set-DataverseAppModuleComponent -PassThru `
   -AppModuleId $appId `
   -ObjectId $view.Id `
   -ComponentType View `
   -RootComponentBehavior IncludeSubcomponents

```

