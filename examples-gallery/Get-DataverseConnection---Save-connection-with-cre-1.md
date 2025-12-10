---
title: "Get-DataverseConnection - Save connection with credentials encrypted (NOT RECOMMENDED)"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Saves the connection with the client secret included (encrypted). WARNING: This stores the secret encrypted on disk and is NOT RECOMMENDED for production use. Only use for testing or non-production scenarios. On Windows, uses DPAPI (Data Protection API); on Linux/macOS, uses AES encryption with machine-specific key.

```powershell
Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "3004eb1e-7a00-45e0-a1dc-6703735eac18" -ClientSecret "itsasecret" -Name "MyOrgService" -SaveCredentials

```
