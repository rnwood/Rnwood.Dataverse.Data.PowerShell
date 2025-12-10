---
title: "Set-DataverseView - Create a basic personal view"
tags: ['Metadata']
source: "Set-DataverseView.md"
---
Creates a personal view (default) showing active contacts with specified columns.

```powershell
Set-DataverseView -PassThru `
   -Name "My Active Contacts" `
   -TableName contact `
 -Columns @("firstname", "lastname", "emailaddress1", "telephone1") `
   -FilterValues @{ statecode = 0 }

```

