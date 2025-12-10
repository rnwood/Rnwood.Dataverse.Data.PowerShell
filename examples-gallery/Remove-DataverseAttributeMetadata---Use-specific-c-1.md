---
title: "Remove-DataverseAttributeMetadata - Use specific connection"
tags: ['Metadata']
source: "Remove-DataverseAttributeMetadata.md"
---
Deletes an attribute using a specific connection.

```powershell
$conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
Remove-DataverseAttributeMetadata onn `
   -EntityName account -AttributeName new_deprecated -Force

```

