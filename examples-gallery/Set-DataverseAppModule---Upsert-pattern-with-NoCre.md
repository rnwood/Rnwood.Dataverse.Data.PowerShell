---
title: "Set-DataverseAppModule - Upsert pattern with NoCreate"
tags: ['Metadata']
source: "Set-DataverseAppModule.md"
---
Updates the app if it exists, but does nothing if it doesn't exist (prevents accidental creation).

```powershell
Set-DataverseAppModule  `
   -UniqueName "existing_app" `
   -Name "Updated Name" `
   -NoCreate

```

