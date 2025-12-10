---
title: "Invoke-DataverseRequest Example"
tags: ['System']
source: "Invoke-DataverseRequest.md"
---
Invokes `myapi_EscalateCase` using the NameAndInputs parameter set. The response is automatically converted to a PSObject with properties accessible directly.

```powershell
$Target = new-object Microsoft.Xrm.Sdk.EntityReference "incident", "{DC66FE5D-B854-4F9D-BA63-4CEA4257A8E9}"
$Priority = new-object Microsoft.Xrm.Sdk.OptionSetValue 1
$response = Invoke-DataverseRequest  myapi_EscalateCase @{
	Target = $Target
	Priority = $Priority
}

```

