---
title: "Remove-DataverseView - Remove view if it exists"
tags: ['Metadata']
source: "Remove-DataverseView.md"
---
Removes a view if it exists, without raising an error if it doesn't exist. Suppresses the confirmation prompt.

```powershell
Remove-DataverseView -Id "12345678-1234-1234-1234-123456789012" -IfExists -Confirm:$false

```

