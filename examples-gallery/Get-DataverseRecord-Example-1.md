---
title: "Get-DataverseRecord Example"
tags: ['Data']
source: "Get-DataverseRecord.md"
---
Find contacts with age greater than 25 by using a nested hashtable to specify operator and value.

```powershell
Get-DataverseRecord -tablename contact -filtervalues @(
	@{
		age = @{
			value = 25
			operator = 'GreaterThan'
		}
	}
)

```

