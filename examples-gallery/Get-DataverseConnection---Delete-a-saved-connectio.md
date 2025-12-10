---
title: "Get-DataverseConnection - Delete a saved connection"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Deletes the saved connection named "MyOrgProd", removing both the connection metadata and cached authentication tokens.

```powershell
Get-DataverseConnection -DeleteConnection -Name "MyOrgProd"

```
