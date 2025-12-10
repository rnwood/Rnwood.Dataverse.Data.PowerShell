---
title: "Get-DataverseEntityMetadata - Compare entity metadata between environments"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Compares entities between development and production environments.

```powershell
$dev = Get-DataverseConnection -Url "https://dev.crm.dynamics.com" -Interactive
$prod = Get-DataverseConnection -Url "https://prod.crm.dynamics.com" -Interactive

$devEntities = Get-DataverseEntityMetadata -Connection $dev | Select-Object -ExpandProperty LogicalName
$prodEntities = Get-DataverseEntityMetadata -Connection $prod | Select-Object -ExpandProperty LogicalName

$onlyInDev = $devEntities | Where-Object { $_ -notin $prodEntities }
$onlyInProd = $prodEntities | Where-Object { $_ -notin $devEntities }

Write-Host "Entities only in Dev: $($onlyInDev -join ', ')"
Write-Host "Entities only in Prod: $($onlyInProd -join ', ')"

```
