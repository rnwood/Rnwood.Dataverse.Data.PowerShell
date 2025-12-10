---
title: "Get-DataverseConnection Example"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Gets a connection to MYORG using a user-assigned managed identity with the specified client ID.

```powershell
$c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ManagedIdentity -ManagedIdentityClientId "12345678-1234-1234-1234-123456789abc"

```
