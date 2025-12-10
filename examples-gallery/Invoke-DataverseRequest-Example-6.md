---
title: "Invoke-DataverseRequest Example"
tags: ['System']
source: "Invoke-DataverseRequest.md"
---
Invokes `WhoAmI` using the NameAndInputs parameter set. The response is automatically converted to a PSObject, so you can access properties directly (e.g., `$response.UserId` instead of `$response.Results["UserId"]`).

```powershell
$response = Invoke-DataverseRequest -requestname "WhoAmI" -parameters @{}
$response.UserId

```

