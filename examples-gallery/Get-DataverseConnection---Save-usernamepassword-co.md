---
title: "Get-DataverseConnection - Save username/password connection with credentials encrypted (NOT RECOMMENDED)"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Saves the connection with the password included (encrypted). WARNING: This stores the password encrypted on disk and is NOT RECOMMENDED for production use. Only use for testing or non-production scenarios.

```powershell
Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -Username "user@domain.com" -Password "mypassword" -Name "MyUserConn" -SaveCredentials

```
