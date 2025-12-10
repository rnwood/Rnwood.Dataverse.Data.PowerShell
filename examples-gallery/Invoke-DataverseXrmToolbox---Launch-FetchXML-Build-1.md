---
title: "Invoke-DataverseXrmToolbox - Launch FetchXML Builder"
tags: ['Tools']
source: "Invoke-DataverseXrmToolbox.md"
---
This example connects to a Dataverse environment and launches the FetchXML Builder plugin with the connection automatically injected.

```powershell
# Connect to Dataverse
$conn = Get-DataverseConnection -Interactive -Url "https://yourorg.crm.dynamics.com"

# Launch FetchXML Builder plugin
Invoke-DataverseXrmToolbox -PackageName "Cinteros.Xrm.FetchXMLBuilder"

```
