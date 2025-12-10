---
title: "Get-DataverseConnection Example"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Gets a connection to MYORG using DefaultAzureCredential, which automatically discovers credentials from the environment (environment variables, managed identity, Visual Studio, Azure CLI, Azure PowerShell, or interactive browser). This is ideal for Azure-hosted applications.

```powershell
$c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -DefaultAzureCredential

```
