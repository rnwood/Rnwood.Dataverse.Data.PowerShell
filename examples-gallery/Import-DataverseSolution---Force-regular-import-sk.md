---
title: "Import-DataverseSolution - Force regular import (skip upgrade logic)"
tags: ['Solutions']
source: "Import-DataverseSolution.md"
---
Imports the solution using regular import, bypassing any upgrade logic. Useful for fresh deployments or when you want to ensure a clean import.

```powershell
Import-DataverseSolution -InFile "C:\Solutions\MySolution.zip" -Mode NoUpgrade

```
