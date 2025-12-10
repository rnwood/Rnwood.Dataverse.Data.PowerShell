---
title: "Set-DataverseEntityKeyMetadata - Create a key and publish the entity"
tags: ['Metadata']
source: "Set-DataverseEntityKeyMetadata.md"
---
This command creates an alternate key and immediately publishes the entity to make the key active.

```powershell
Set-DataverseEntityKeyMetadata -EntityName account -SchemaName "account_number_key" -KeyAttributes @("accountnumber") -Publish

```

