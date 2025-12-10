---
title: "Get-DataverseConnection Example"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Gets a connection to MYORG using interactive authentication and stores the result in the `$c` variable for later use.

```powershell
$c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -Interactive

```
