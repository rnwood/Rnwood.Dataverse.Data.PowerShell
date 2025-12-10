---
title: "Clear-DataverseMetadataCache - Clear cache for a specific connection"
tags: ['System']
source: "Clear-DataverseMetadataCache.md"
---
Clears the metadata cache only for the specified connection.

```powershell
$conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
Clear-DataverseMetadataCache onn

```

