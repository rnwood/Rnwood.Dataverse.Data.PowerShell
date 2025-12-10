---
title: "Remove-DataverseAppModuleComponent - Remove component by app module ID"
tags: ['Metadata']
source: "Remove-DataverseAppModuleComponent.md"
---
Removes a form from an app module by specifying the app module ID and form ID.

```powershell
$app = Get-DataverseAppModule -UniqueName "myapp"
$form = Get-DataverseRecord -TableName systemform -FilterValues @{ name = "Contact Form" }
Remove-DataverseAppModuleComponent -AppModuleId $app.Id -ObjectId $form.systemformid -Confirm:$false

```

