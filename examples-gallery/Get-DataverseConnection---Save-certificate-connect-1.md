---
title: "Get-DataverseConnection - Save certificate connection with credentials encrypted (NOT RECOMMENDED)"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Saves the connection with certificate path and password included (encrypted). WARNING: This stores the password encrypted on disk and is NOT RECOMMENDED for production use. Only use for testing or non-production scenarios.

```powershell
Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "12345678-1234-1234-1234-123456789abc" -CertificatePath "C:\certs\mycert.pfx" -CertificatePassword "P@ssw0rd" -Name "MyCertConn" -SaveCredentials

```
