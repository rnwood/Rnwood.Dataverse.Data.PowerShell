---
title: "Get-DataverseOptionSetMetadata - Compare option sets between environments"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Compares option set values between development and production environments.

```powershell
$dev = Get-DataverseConnection -Url "https://dev.crm.dynamics.com" -Interactive
$prod = Get-DataverseConnection -Url "https://prod.crm.dynamics.com" -Interactive

$devOptions = Get-DataverseOptionSetMetadata -Connection $dev -EntityName account -AttributeName industrycode
$prodOptions = Get-DataverseOptionSetMetadata -Connection $prod -EntityName account -AttributeName industrycode

$devValues = $devOptions.Options.Value
$prodValues = $prodOptions.Options.Value

$onlyInDev = $devValues | Where-Object { $_ -notin $prodValues }
Write-Host "Options only in Dev: $($onlyInDev -join ', ')"

```
