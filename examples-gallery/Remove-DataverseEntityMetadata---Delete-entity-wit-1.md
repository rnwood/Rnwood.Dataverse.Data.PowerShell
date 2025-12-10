---
title: "Remove-DataverseEntityMetadata - Delete entity with specific connection"
tags: ['Metadata']
source: "Remove-DataverseEntityMetadata.md"
---
Deletes an entity using a specific connection.

```powershell
$conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
Remove-DataverseEntityMetadata onn -EntityName new_oldentity -Force

```

