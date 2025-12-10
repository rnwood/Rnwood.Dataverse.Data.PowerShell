---
title: "Set-DataverseAppModule - Create with navigation type and featured setting"
tags: ['Metadata']
source: "Set-DataverseAppModule.md"
---
Creates a new app module with multi-session navigation and marks it as featured.

```powershell
Set-DataverseAppModule -PassThru `
   -UniqueName "featured_app" `
   -Name "Featured Application" `
   -NavigationType MultiSession `
   -IsFeatured $true

```

