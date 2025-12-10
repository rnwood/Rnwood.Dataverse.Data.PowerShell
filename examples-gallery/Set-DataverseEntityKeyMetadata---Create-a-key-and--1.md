---
title: "Set-DataverseEntityKeyMetadata - Create a key and return the metadata"
tags: ['Metadata']
source: "Set-DataverseEntityKeyMetadata.md"
---
This command creates an alternate key and returns the key metadata object for further inspection.

```powershell
$key = Set-DataverseEntityKeyMetadata -EntityName contact -SchemaName "contact_email_key" -KeyAttributes @("emailaddress1") -PassThru
$key | Format-List

```

