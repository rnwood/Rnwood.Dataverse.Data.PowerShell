---
title: "Invoke-DataverseXrmToolbox - Use custom cache directory"
tags: ['Tools']
source: "Invoke-DataverseXrmToolbox.md"
---
This example uses a custom directory for caching downloaded plugins.

```powershell
# Use a custom cache directory
Invoke-DataverseXrmToolbox `
   -PackageName "MsCrmTools.WebResourcesManager" `
   -CacheDirectory "C:\MyPluginCache"

```
