---
title: "Set-DataverseAttributeMetadata - Update with specific connection"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Updates an attribute using a specific connection instead of the default connection.

```powershell
$conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive

Set-DataverseAttributeMetadata onn -EntityName account `
   -AttributeName new_customfield -DisplayName "Updated Name"

```

