---
title: "Get-DataverseFormTab - Pipeline usage with form objects"
tags: ['Metadata']
source: "Get-DataverseFormTab.md"
---
Gets all tabs from all account forms using pipeline.

```powershell
Get-DataverseForm -Entity 'account' | 
    ForEach-Object { Get-DataverseFormTab -FormId $_.Id }

```

