---
title: "Invoke-DataverseRequest Example"
tags: ['System']
source: "Invoke-DataverseRequest.md"
---
Invokes the `POST` `myapi_Example` REST API using custom headers and body. REST responses are returned as JSON objects without conversion.

```powershell
invoke-dataverserequest -method POST myapi_Example \
	-CustomHeaders @{
		foo = "bar"
	} \
	-Body @{
		a = 1
		b = 3
	}

```

