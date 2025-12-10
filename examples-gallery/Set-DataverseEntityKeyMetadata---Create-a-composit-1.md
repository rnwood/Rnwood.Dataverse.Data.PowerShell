---
title: "Set-DataverseEntityKeyMetadata - Create a composite key with multiple attributes"
tags: ['Metadata']
source: "Set-DataverseEntityKeyMetadata.md"
---
This command creates an alternate key using both firstname and lastname attributes with a custom display name.

```powershell
Set-DataverseEntityKeyMetadata -EntityName contact -SchemaName "contact_name_key" -KeyAttributes @("firstname", "lastname") -DisplayName "Full Name Key"

```

