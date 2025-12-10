---
title: "Invoke-DataverseSql Example"
tags: ['Data']
source: "Invoke-DataverseSql.md"
---
Returns the rows from the SELECT query matching the @lastname parameter which is supplied.

```powershell
Invoke-DataverseSql -sql "SELECT TOP 1 createdon FROM Contact WHERE lastname=@lastname" -parameters @{
	lastname = "Wood"
}

# createdon
# ---------
# 28/11/2024 16:28:12

```

