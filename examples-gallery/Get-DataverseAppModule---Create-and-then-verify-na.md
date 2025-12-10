---
title: "Get-DataverseAppModule - Create and then verify navigation type & featured flag"
tags: ['Metadata']
source: "Get-DataverseAppModule.md"
---
Creates an app module with multi-session navigation and verifies the values.

```powershell
$id = Set-DataverseAppModule -PassThru -UniqueName "multi_session_app" -Name "Multi Session" -NavigationType MultiSession -IsFeatured $true
Get-DataverseAppModule -Id $id | Select-Object UniqueName, NavigationType, IsFeatured

```
