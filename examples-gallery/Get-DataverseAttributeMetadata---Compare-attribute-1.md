---
title: "Get-DataverseAttributeMetadata - Compare attribute metadata between entities"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Compares attributes between two entities.

```powershell
$accountAttrs = Get-DataverseAttributeMetadata -EntityName account
$contactAttrs = Get-DataverseAttributeMetadata -EntityName contact

$accountNames = $accountAttrs.LogicalName
$contactNames = $contactAttrs.LogicalName

$commonAttrs = $accountNames | Where-Object { $_ -in $contactNames }
Write-Host "Common attributes: $($commonAttrs.Count)"
# Common attributes: 45

```
