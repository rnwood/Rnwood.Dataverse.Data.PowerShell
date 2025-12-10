---
title: "Set-DataverseView - Clone a view using Get-DataverseView output"
tags: ['Metadata']
source: "Set-DataverseView.md"
---
Retrieves a view and creates a copy. The properties returned by Get-DataverseView (Columns, Filters, Links, OrderBy) are in the format expected by Set-DataverseView.

```powershell
$originalView = Get-DataverseView -Id $originalViewId
Set-DataverseView -PassThru `
   -Name "$($originalView.Name) (Copy)" `
   -TableName $originalView.TableName `
   -ViewType $originalView.ViewType `
   -Columns $originalView.Columns `
   -FilterValues $originalView.Filters `
   -Links $originalView.Links `
   -OrderBy $originalView.OrderBy

```

