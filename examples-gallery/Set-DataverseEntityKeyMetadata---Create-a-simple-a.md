---
title: "Set-DataverseEntityKeyMetadata - Create a simple alternate key"
tags: ['Metadata']
source: "Set-DataverseEntityKeyMetadata.md"
---
This command creates an alternate key on the contact entity using the emailaddress1 attribute.

```powershell
$connection = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
Set-DataverseEntityKeyMetadata -EntityName contact -SchemaName "contact_emailaddress1_key" -KeyAttributes @("emailaddress1")

```

