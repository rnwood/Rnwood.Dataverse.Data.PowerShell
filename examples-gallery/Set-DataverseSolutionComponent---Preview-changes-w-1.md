---
title: "Set-DataverseSolutionComponent - Preview changes with WhatIf"
tags: ['Solutions']
source: "Set-DataverseSolutionComponent.md"
---
Shows what would happen without making changes.

```powershell
Set-DataverseSolutionComponent -SolutionName "MySolution" `
   -ComponentId "dfe12c85-55b3-4c77-9c04-7d5d06d2e9e2" -ComponentType 1 -Behavior 2 -WhatIf

# What if: Performing the operation "Add to solution 'MySolution' with behavior: Include As Shell" 
# on target "Component dfe12c85-55b3-4c77-9c04-7d5d06d2e9e2 (type 1)".

```

