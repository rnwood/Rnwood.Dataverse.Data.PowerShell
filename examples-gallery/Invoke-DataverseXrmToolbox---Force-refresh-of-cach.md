---
title: "Invoke-DataverseXrmToolbox - Force refresh of cached plugin"
tags: ['Tools']
source: "Invoke-DataverseXrmToolbox.md"
---
This example forces a re-download of the plugin, even if it's already cached locally.

```powershell
# Force re-download of the plugin
Invoke-DataverseXrmToolbox -PackageName "Cinteros.Xrm.FetchXMLBuilder" -Force

```
