---
title: "Set-DataverseSolutionComponent - Change component behavior from Include to Do Not Include Subcomponents"
tags: ['Solutions']
source: "Set-DataverseSolutionComponent.md"
---
Changes the behavior of an existing component. The cmdlet removes and re-adds the component with the new behavior, returning details via PassThru.

```powershell
Set-DataverseSolutionComponent -SolutionName "MySolution" `
   -ComponentId "dfe12c85-55b3-4c77-9c04-7d5d06d2e9e2" -ComponentType 1 -Behavior 1 -PassThru

# SolutionName  : MySolution
# SolutionId    : a1b2c3d4-5678-90ab-cdef-1234567890ab
# ComponentId   : dfe12c85-55b3-4c77-9c04-7d5d06d2e9e2
# ComponentType : 1
# Behavior      : Do Not Include Subcomponents
# BehaviorValue : 1
# WasUpdated    : True

```

