---
title: "Get-DataverseConnection Example"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Authenticates interactively without specifying a URL. The cmdlet will automatically display a list of available Dataverse environments for the user to select from. This is useful when you have access to multiple environments and don't want to manually specify the URL.

```powershell
$c = Get-DataverseConnection -Interactive

```
