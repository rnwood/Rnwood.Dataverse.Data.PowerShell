---
title: "Remove-DataverseEntityKeyMetadata - Delete key from pipeline"
tags: ['Metadata']
source: "Remove-DataverseEntityKeyMetadata.md"
---
This command demonstrates piping the entity name to the cmdlet.

```powershell
"contact" | Remove-DataverseEntityKeyMetadata -KeyName "contact_phone_key" -Confirm:$false

```

