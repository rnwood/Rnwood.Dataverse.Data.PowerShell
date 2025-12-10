---
title: "Remove-DataverseRelationshipMetadata - Use specific connection"
tags: ['Metadata']
source: "Remove-DataverseRelationshipMetadata.md"
---
Deletes a relationship using a specific connection.

```powershell
$conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
Remove-DataverseRelationshipMetadata onn `
   -SchemaName new_deprecated_rel -Force

```

