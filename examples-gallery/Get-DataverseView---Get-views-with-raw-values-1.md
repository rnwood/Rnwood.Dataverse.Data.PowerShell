---
title: "Get-DataverseView - Get views with raw values"
tags: ['Metadata']
source: "Get-DataverseView.md"
---
Retrieves a view with raw attribute values instead of parsed properties. Returns all attributes from the savedquery/userquery record including fetchxml, layoutxml, querytype, etc.

```powershell
Get-DataverseView -Id $viewId -Raw

```

