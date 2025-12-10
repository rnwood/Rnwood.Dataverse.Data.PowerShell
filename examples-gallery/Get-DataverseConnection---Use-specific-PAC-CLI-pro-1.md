---
title: "Get-DataverseConnection - Use specific PAC CLI profile by name or index"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Connects to Dataverse using a specific named PAC CLI profile. The profile name must match one of the profiles created with `pac auth create --name <profilename>`. Alternatively, you can specify the index of the profile (e.g., "0" for the first profile).

```powershell
$c = Get-DataverseConnection -FromPac -Profile "MyDevProfile"

```
