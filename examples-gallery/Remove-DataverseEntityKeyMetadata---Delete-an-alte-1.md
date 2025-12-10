---
title: "Remove-DataverseEntityKeyMetadata - Delete an alternate key"
tags: ['Metadata']
source: "Remove-DataverseEntityKeyMetadata.md"
---
This command deletes the alternate key named "contact_emailaddress1_key" from the contact entity without prompting for confirmation.

```powershell
$connection = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
Remove-DataverseEntityKeyMetadata -EntityName contact -KeyName "contact_emailaddress1_key" -Confirm:$false

```

