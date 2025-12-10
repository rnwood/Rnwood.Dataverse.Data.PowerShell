---
title: "Get-DataverseFormTab - Get a specific tab by name"
tags: ['Metadata']
source: "Get-DataverseFormTab.md"
---
Retrieves only the 'General' tab from the specified form.

```powershell
$formId = '12345678-1234-1234-1234-123456789012'
Get-DataverseFormTab -FormId $formId -TabName 'General'

```

