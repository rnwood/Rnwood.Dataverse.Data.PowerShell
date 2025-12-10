---
title: "Get-DataverseConnection Example"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Gets a connection to MYORG using a script block that returns an access token. The script block is called whenever a new access token is needed. This is useful for custom authentication scenarios where you manage token acquisition externally.

```powershell
$c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -AccessToken { "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im5PbzNaRHJ1VW1sMGhrR2ZaRjBqZWFHWW9XQT..." }

```
