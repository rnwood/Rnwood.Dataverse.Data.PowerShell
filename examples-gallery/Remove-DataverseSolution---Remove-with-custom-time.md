---
title: "Remove-DataverseSolution - Remove with custom timeout"
tags: ['Solutions']
source: "Remove-DataverseSolution.md"
---
Removes a large solution with a 20-minute timeout and checks status every 10 seconds.

```powershell
Remove-DataverseSolution -UniqueName "LargeSolution" -TimeoutSeconds 1200 -PollingIntervalSeconds 10
# Solution 'Large Solution' removed successfully.

```
