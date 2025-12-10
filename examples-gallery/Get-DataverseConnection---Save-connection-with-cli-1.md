---
title: "Get-DataverseConnection - Save connection with client secret"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Connects using client secret authentication and saves the connection as "MyOrgService". Note: The client secret itself is NOT saved for security reasons. When loading this connection later, you will need to provide the client secret again.

```powershell
Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "3004eb1e-7a00-45e0-a1dc-6703735eac18" -ClientSecret "itsasecret" -Name "MyOrgService"

```
