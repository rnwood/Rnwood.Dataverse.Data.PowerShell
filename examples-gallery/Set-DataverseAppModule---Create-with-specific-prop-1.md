---
title: "Set-DataverseAppModule - Create with specific properties"
tags: ['Metadata']
source: "Set-DataverseAppModule.md"
---
Creates a new app module with icon, URL, and form factor settings.

```powershell
$webResourceId = (Get-DataverseRecord -TableName webresource -FilterValues @{name="app_icon.svg"}).webresourceid
Set-DataverseAppModule -PassThru `
   -UniqueName "sales_app" `
   -Name "Sales Application" `
   -Url "/main.aspx?app=sales" `
   -WebResourceId $webResourceId `
   -FormFactor 1 `
   -ClientType 1

```

