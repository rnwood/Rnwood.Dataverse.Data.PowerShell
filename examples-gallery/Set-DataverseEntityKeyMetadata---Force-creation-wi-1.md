---
title: "Set-DataverseEntityKeyMetadata - Force creation without existence check"
tags: ['Metadata']
source: "Set-DataverseEntityKeyMetadata.md"
---
This command creates an alternate key while skipping the existence check. Use this with caution as it may cause errors if the key already exists.

```powershell
Set-DataverseEntityKeyMetadata -EntityName contact -SchemaName "contact_phone_key" -KeyAttributes @("telephone1") -Force

```

