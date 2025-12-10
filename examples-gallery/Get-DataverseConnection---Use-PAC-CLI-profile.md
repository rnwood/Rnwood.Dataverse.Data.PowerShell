---
title: "Get-DataverseConnection - Use PAC CLI profile"
tags: ['Connection']
source: "Get-DataverseConnection.md"
---
Connects to Dataverse using the current Power Platform CLI (PAC) authentication profile. This leverages the authentication you've already established with `pac auth create` and will use the currently selected environment (set via `pac org select`).

```powershell
$c = Get-DataverseConnection -FromPac

```
