---
title: "Invoke-DataverseSql Example"
tags: ['Data']
source: "Invoke-DataverseSql.md"
---
Returns the rows from the SELECT query matching the @lastname parameters which are supplied via the pipeline. The query is executed once for each of the pipeline objects.

```powershell
@(
)	@{
		lastname = "Wood"
	},
	@{
		lastname = "Cat2"
	}
) | Invoke-DataverseSql -sql "SELECT TOP 1 lastname, createdon FROM Contact WHERE lastname=@lastname"

# lastname createdon
# -------- ---------
# Wood     28/11/2024 16:28:12
# Cat2     28/11/2024 16:42:30

```

