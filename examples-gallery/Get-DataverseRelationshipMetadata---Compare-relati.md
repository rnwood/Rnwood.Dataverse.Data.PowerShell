---
title: "Get-DataverseRelationshipMetadata - Compare relationships between environments"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Compares relationships between development and production environments.

```powershell
$conn1 = Get-DataverseConnection -Url "https://dev.crm.dynamics.com" -Interactive
$conn2 = Get-DataverseConnection -Url "https://prod.crm.dynamics.com" -Interactive

$devRels = Get-DataverseRelationshipMetadata -Connection $conn1 -EntityName account
$prodRels = Get-DataverseRelationshipMetadata -Connection $conn2 -EntityName account

$devSchemas = $devRels | Select-Object -ExpandProperty SchemaName
$prodSchemas = $prodRels | Select-Object -ExpandProperty SchemaName

$onlyInDev = $devSchemas | Where-Object { $_ -notin $prodSchemas }
$onlyInProd = $prodSchemas | Where-Object { $_ -notin $devSchemas }

Write-Host "Relationships only in Dev: $($onlyInDev -join ', ')"
Write-Host "Relationships only in Prod: $($onlyInProd -join ', ')"

```
