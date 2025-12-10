---
title: "Invoke-DataverseRequest - Using retry logic for transient failures"
tags: ['System']
source: "Invoke-DataverseRequest.md"
---
Invokes a WhoAmI request with automatic retry on transient failures. Failed requests will be retried up to 3 times with delays of 5s, 10s, and 20s respectively. The `-Verbose` flag shows retry attempts and wait times.

```powershell
$request = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
$response = Invoke-DataverseRequest -Request $request -Retries 3 -InitialRetryDelay 5 -Verbose

```

