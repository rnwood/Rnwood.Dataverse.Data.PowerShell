---
title: "Get-DataverseFormControl - Find all email controls across the entire form"
tags: ['Metadata']
source: "Get-DataverseFormControl.md"
---
Finds all email-type controls in the form, including those in the header and regular sections.

```powershell
$controls = Get-DataverseFormControl -FormId $formId
$emailControls = $controls | Where-Object { $_.ClassId -match 'ADA2203E-B4CD' }
$emailControls | Select-Object TabName, SectionName, DataField

```

