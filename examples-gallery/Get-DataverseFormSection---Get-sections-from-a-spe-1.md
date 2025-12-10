---
title: "Get-DataverseFormSection - Get sections from a specific tab"
tags: ['Metadata']
source: "Get-DataverseFormSection.md"
---
Retrieves all sections from the 'General' tab of the specified form.

```powershell
$formId = '12345678-1234-1234-1234-123456789012'
Get-DataverseFormSection -FormId $formId -TabName 'General'

```

