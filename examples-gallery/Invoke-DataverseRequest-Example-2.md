---
title: "Invoke-DataverseRequest Example"
tags: ['System']
source: "Invoke-DataverseRequest.md"
---
Invokes `WhoAmI` using the NameAndInputs parameter set with `-Raw` switch. The response is returned as a raw `OrganizationResponse` without conversion.

```powershell
$response = Invoke-DataverseRequest -requestname "WhoAmI" -parameters @{} -Raw
$response.Results["UserId"]

```

