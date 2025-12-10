---
title: "Get-DataverseConnection Example"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Gets a connection to MYORG using client certificate authentication with a certificate from the LocalMachine\Root certificate store.

```powershell
$c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "12345678-1234-1234-1234-123456789abc" -CertificateThumbprint "A1B2C3D4E5F6789012345678901234567890ABCD" -CertificateStoreLocation LocalMachine -CertificateStoreName Root

```
