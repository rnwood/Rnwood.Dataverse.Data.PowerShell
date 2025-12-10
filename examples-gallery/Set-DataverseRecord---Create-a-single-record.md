---
title: "Set-DataverseRecord - Create a single record"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Creates a new contact record with a last name and first name.

```powershell
[PSCustomObject] @{
	TableName = "contact"
	lastname = "Simpson"
	firstname = "Homer"
} | Set-DataverseRecord 

```

