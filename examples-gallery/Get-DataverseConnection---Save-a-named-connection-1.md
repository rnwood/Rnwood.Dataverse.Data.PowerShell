---
title: "Get-DataverseConnection - Save a named connection"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Connects to MYORG using interactive authentication and saves the connection with the name "MyOrgProd". The authentication tokens will be cached securely, and connection metadata will be saved for later use.

```powershell
Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -Interactive -Name "MyOrgProd"

```
