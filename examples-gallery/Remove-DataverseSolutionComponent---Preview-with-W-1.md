---
title: "Remove-DataverseSolutionComponent - Preview with WhatIf"
tags: ['Solutions']
source: "Remove-DataverseSolutionComponent.md"
---
Shows what would happen without actually removing the component.

```powershell
Remove-DataverseSolutionComponent -SolutionName "MySolution" `
   -ComponentId $componentId -ComponentType 1 -WhatIf

# What if: Performing the operation "Remove from solution 'MySolution'" 
# on target "Component dfe12c85-55b3-4c77-9c04-7d5d06d2e9e2 (type 1)".

```

