---
title: "Remove-DataverseAppModule - Safe removal with IfExists"
tags: ['Metadata']
source: "Remove-DataverseAppModule.md"
---
Attempts to remove the app but doesn't error if it doesn't exist.

```powershell
Remove-DataverseAppModule -UniqueName "maybe_exists" -IfExists -Confirm:$false

```

