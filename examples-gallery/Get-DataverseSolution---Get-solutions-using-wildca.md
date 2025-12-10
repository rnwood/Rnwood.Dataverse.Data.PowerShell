---
title: "Get-DataverseSolution - Get solutions using wildcard patterns"
tags: ['Solutions']
source: "Get-DataverseSolution.md"
---
Retrieves all solutions whose unique name starts with "Contoso".

```powershell
Get-DataverseSolution -UniqueName "Contoso*"

# UniqueName            Name                  Version    IsManaged
# ----------           ----                 -------   ---------
# ContosoSales          Contoso Sales         1.0.0.0    False
# ContosoMarketing      Contoso Marketing     2.1.0.0    False

```
