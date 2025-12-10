---
title: "Invoke-DataverseRequest - Calling a custom API on a specific record"
tags: ['System']
source: "Invoke-DataverseRequest.md"
---
Invokes a custom API on a specific record using a navigation path. This pattern is useful for calling bound custom actions that operate on a specific entity instance.

```powershell
$id = "1d936fda-9076-ef11-a671-6045bd0ab99c"
$response = Invoke-DataverseRequest -method POST -path "sample_entities($id)/Microsoft.Dynamics.CRM.sample_MyCustomApi" -body @{ param1 = "value1" }

```

