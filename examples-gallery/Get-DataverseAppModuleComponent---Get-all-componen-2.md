---
title: "Get-DataverseAppModuleComponent - Get all components for an app module by ID"
tags: ['Metadata']
source: "Get-DataverseAppModuleComponent.md"
---
Retrieves all components associated with a specific app module using its ID.

---

```powershell
$app = Get-DataverseAppModule -UniqueName "myapp"
Get-DataverseAppModuleComponent -AppModuleId $app.Id

```
