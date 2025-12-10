---
title: "Get-DataverseSolution - Filter solutions with multiple wildcards"
tags: ['Solutions']
source: "Get-DataverseSolution.md"
---
Retrieves all unmanaged solutions whose unique name contains "Custom".

```powershell
Get-DataverseSolution -UniqueName "*Custom*" -Unmanaged

# UniqueName            Name                  Version    IsManaged
# ----------           ----                 -------   ---------
# MyCustomSolution      My Custom Solution    1.0.0.0    False
# TeamCustomization     Team Customization    1.5.0.0    False

```
