---
title: "Set-DataverseSolution - Example4: Attempt to update managed solution (shows warning)"
tags: ['Solutions']
source: "Set-DataverseSolution.md"
---
Attempts to update a managed solution, but only description updates are allowed.

```powershell
Set-DataverseSolution -UniqueName "ManagedSolution" -Name "New Name" -Version "2.0.0.0"
WARNING: Solution is managed. Only the description can be updated for managed solutions.
WARNING: Cannot update name of managed solution. Skipping name update.
WARNING: Cannot update version of managed solution. Skipping version update.
WARNING: No updates to apply. Please specify at least one property to update (Name, Description, or Version).

```

