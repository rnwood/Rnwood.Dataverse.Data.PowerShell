---
title: "Get-DataverseConnection Example"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Gets a connection to MYORG using client certificate authentication with a certificate file. The certificate file is password-protected.

```powershell
$c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "12345678-1234-1234-1234-123456789abc" -CertificatePath "C:\certs\mycert.pfx" -CertificatePassword "P@ssw0rd"

```
