---
title: "Invoke-DataverseRequest Example"
tags: ['System']
source: "Invoke-DataverseRequest.md"
---
Invokes `WhoAmIRequest` using the Request parameter set. The response is a raw `WhoAmIResponse` object. Access properties via the `Results` collection.

```powershell
$request = new-object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
$response = Invoke-DataverseRequest -request $request
$response.Results["UserId"]

```

