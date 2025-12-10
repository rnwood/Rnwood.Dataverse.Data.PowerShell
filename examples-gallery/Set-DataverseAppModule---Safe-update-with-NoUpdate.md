---
title: "Set-DataverseAppModule - Safe update with NoUpdate"
tags: ['Metadata']
source: "Set-DataverseAppModule.md"
---
Returns the existing app ID without making any changes (prevents accidental updates).

```powershell
Set-DataverseAppModule  `
   -Id $appId `
   -Name "New Name" `
   -NoUpdate

```

