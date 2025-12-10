---
title: "Get-DataverseRecord Example"
tags: ['Data']
source: "Get-DataverseRecord.md"
---
Get all contacts where firstname starts with 'Rob' and return the firstname column only.

```powershell
Get-DataverseRecord -tablename contact -columns firstname -filtervalues @{
	"firstname:Like" = "Rob%"
}

```

