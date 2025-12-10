---
title: "Remove-DataverseAppModuleComponent - Remove with IfExists flag"
tags: ['Metadata']
source: "Remove-DataverseAppModuleComponent.md"
---
Attempts to remove a component but doesn't error if it doesn't exist.

```powershell
Remove-DataverseAppModuleComponent -Id $componentId -IfExists -Confirm:$false

```

