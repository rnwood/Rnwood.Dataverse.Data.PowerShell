---
title: "Get-DataverseConnection Example"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Gets a connection to MYORG using the system-assigned managed identity. This is useful when running in Azure environments like Azure Functions, App Service, or VMs with managed identity enabled.

```powershell
$c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ManagedIdentity

```
