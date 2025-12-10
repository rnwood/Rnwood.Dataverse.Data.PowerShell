---
title: "Set-DataverseAppModuleComponent - Update component properties"
tags: ['Metadata']
source: "Set-DataverseAppModuleComponent.md"
---
Updates an existing component to be the default with DoNotIncludeSubcomponents behavior.

```powershell
Set-DataverseAppModuleComponent  `
   -Id $componentId `
   -IsDefault $true `
   -RootComponentBehavior DoNotIncludeSubcomponents

```

