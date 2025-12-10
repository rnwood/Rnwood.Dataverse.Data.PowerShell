---
title: "Set-DataverseView - Create a system view with column widths"
tags: ['Metadata']
source: "Set-DataverseView.md"
---
Creates a system view accessible to all users with specific column widths.

```powershell
Set-DataverseView -PassThru `
 -Name "All Active Contacts" `
   -TableName contact `
   -ViewType "System" `
 -Columns @(
 @{ name = "firstname"; width = 100 },
        @{ name = "lastname"; width = 150 },
      @{ name = "emailaddress1"; width = 250 },
  @{ name = "telephone1"; width = 120 }
    ) `
   -FilterValues @{ statecode = 0 }

```

