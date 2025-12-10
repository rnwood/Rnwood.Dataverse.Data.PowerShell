---
title: "Sets the interactively authenticated connection as the default."
tags: ['Connection']
source: "Set-DataverseConnectionAsDefault.md"
---
Sets the interactively authenticated connection as the default.

```powershell
$connection = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
Set-DataverseConnectionAsDefault

```

