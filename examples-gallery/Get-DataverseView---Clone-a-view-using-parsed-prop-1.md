---
title: "Get-DataverseView - Clone a view using parsed properties"
tags: ['Metadata']
source: "Get-DataverseView.md"
---
Gets a view and uses its parsed properties to create a clone. The Columns property includes name and width configuration in the format expected by Set-DataverseView.

```powershell
$view = Get-DataverseView -Id $viewId
Set-DataverseView -PassThru `
   -Name "$($view.Name) (Copy)" `
   -TableName $view.TableName `
   -ViewType $view.ViewType `
   -Columns $view.Columns `
   -FilterValues $view.Filters `
   -Links $view.Links `
   -OrderBy $view.OrderBy

```

