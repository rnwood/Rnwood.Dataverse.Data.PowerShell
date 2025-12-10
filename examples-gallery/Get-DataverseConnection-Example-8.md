---
title: "Get-DataverseConnection Example"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Gets a connection to MYORG using client certificate authentication with a certificate from the Windows certificate store (CurrentUser\My by default).

```powershell
$c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "12345678-1234-1234-1234-123456789abc" -CertificateThumbprint "A1B2C3D4E5F6789012345678901234567890ABCD"

```
