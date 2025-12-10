---
title: "Set-DataverseAppModule - Create and publish an app module"
tags: ['Metadata']
source: "Set-DataverseAppModule.md"
---
Creates a new app module, validates it, and publishes it immediately.

```powershell
Set-DataverseAppModule -PassThru `
   -UniqueName "ready_app" `
   -Name "Ready Application" `
   -Validate `
   -Publish

```

