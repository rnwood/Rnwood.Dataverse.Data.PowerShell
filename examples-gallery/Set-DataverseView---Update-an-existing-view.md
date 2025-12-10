---
title: "Set-DataverseView - Update an existing view"
tags: ['Metadata']
source: "Set-DataverseView.md"
---
Updates the name and description of an existing view. ViewType must be specified when updating.

```powershell
Set-DataverseView -Id $viewId `
   -ViewType "Personal" `
   -Name "Updated Contact View" `
   -Description "Shows active contacts with email"

```

