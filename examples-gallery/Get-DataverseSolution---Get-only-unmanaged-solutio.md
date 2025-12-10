---
title: "Get-DataverseSolution - Get only unmanaged solutions, excluding system solutions"
tags: ['Solutions']
source: "Get-DataverseSolution.md"
---
Retrieves only unmanaged solutions, excluding the Default, Active, and Basic system solutions.

```powershell
Get-DataverseSolution -Unmanaged -ExcludeSystemSolutions

# UniqueName            Name                  Version    IsManaged
# ----------           ----                 -------   ---------
# MySolution            My Solution           1.0.0.0    False
# CustomSolution        Custom Solution       1.2.0.0    False

```
