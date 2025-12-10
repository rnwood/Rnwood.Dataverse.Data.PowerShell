---
title: "Get-DataverseConnection Example"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Gets a connection to MYORG using client certificate authentication with an unencrypted certificate file (no password required).

```powershell
$c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "12345678-1234-1234-1234-123456789abc" -CertificatePath "./mycert.pfx"

```
