---
title: "Set-DataverseAppModuleComponent - Add a form component"
tags: ['Metadata']
source: "Set-DataverseAppModuleComponent.md"
---
Adds a form as the default form for the app.

```powershell
$form = Get-DataverseRecord -TableName systemform -FilterValues @{ name = "Contact Main Form" }
Set-DataverseAppModuleComponent -PassThru `
   -AppModuleUniqueName "myapp" `
   -ObjectId $form.systemformid `
   -ComponentType Form `
   -IsDefault $true

```

