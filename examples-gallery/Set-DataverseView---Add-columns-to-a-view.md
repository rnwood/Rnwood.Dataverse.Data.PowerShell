---
title: "Set-DataverseView - Add columns to a view"
tags: ['Metadata']
source: "Set-DataverseView.md"
---
Adds new columns to an existing view without affecting existing columns.

```powershell
Set-DataverseView -Id $viewId `
   -ViewType "Personal" `
   -AddColumns @(
        @{ name = "address1_city"; width = 150 },
        @{ name = "birthdate"; width = 100 }
    )

```

