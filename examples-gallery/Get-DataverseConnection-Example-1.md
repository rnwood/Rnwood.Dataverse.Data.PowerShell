---
title: "Get-DataverseConnection Example"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Gets a connection to MYORG using Service Principal client ID and secret auth and stores the result in the `$c` variable for later use.

```powershell
$c = Get-DataverseConnection -url "https://myorg.crm4.dynamics.com" -clientid "3004eb1e-7a00-45e0-a1dc-6703735eac18" -clientsecret "itsasecret"

```
