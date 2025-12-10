---
title: "Compare-DataverseSolutionComponents - View subcomponents of entities"
tags: ['Solutions']
source: "Compare-DataverseSolutionComponents.md"
---
This example shows only subcomponents (attributes, views, forms, etc.) and their parent component types.

```powershell
$results = Compare-DataverseSolutionComponents -SolutionFile "C:\Solutions\MySolution.zip"
$results | Where-Object IsSubcomponent -eq $true | Format-Table ComponentTypeName, Status, ParentComponentTypeName

```
