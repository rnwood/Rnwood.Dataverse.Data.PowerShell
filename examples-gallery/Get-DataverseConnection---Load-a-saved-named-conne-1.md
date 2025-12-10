---
title: "Get-DataverseConnection - Load a saved named connection"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Loads the previously saved connection named "MyOrgProd". The cmdlet will use the cached authentication tokens, avoiding the need to authenticate again unless the tokens have expired.

```powershell
$c = Get-DataverseConnection -Name "MyOrgProd"

```
