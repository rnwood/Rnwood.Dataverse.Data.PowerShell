---
title: "Get-DataverseEntityKeyMetadata - Get all alternate keys for an entity"
tags: ['Metadata']
source: "Get-DataverseEntityKeyMetadata.md"
---
This command retrieves all alternate keys defined on the contact entity.

```powershell
$connection = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
Get-DataverseEntityKeyMetadata -EntityName contact

```

